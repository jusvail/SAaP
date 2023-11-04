using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;
using SAaP.Core.Helpers;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;
using SAaP.Models;

namespace SAaP.ViewModels;

public partial class StatisticsViewModel : ObservableRecipient
{
	[ObservableProperty] private bool _analysisStarted;
	[ObservableProperty] private bool _isAnalysisStarted;
	[ObservableProperty] private string _analysisBtnStatus;
	[ObservableProperty] private string _codeInput;
	[ObservableProperty] private string _currentStatus;

	public ObservableStatisticsDetail TaskDetail { get; set; } = new();

	private CancellationTokenSource _cts;
	private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
	private readonly IFetchStockDataService _fetchStockDataService;
	private readonly IDbTransferService _dbTransferService;
	private readonly IStockAnalyzeService _stockAnalyzeService;
	private readonly IChartService _chartService;

	public StatisticsViewModel(IFetchStockDataService fetchStockDataService, IDbTransferService dbTransferService, IStockAnalyzeService stockAnalyzeService, IChartService chartService)
	{
		_fetchStockDataService = fetchStockDataService;
		_dbTransferService = dbTransferService;
		_stockAnalyzeService = stockAnalyzeService;
		_chartService = chartService;
	}

	#region common

	private void SetCurrentStatus(string status)
	{
		SetValueCrossThread(() => { CurrentStatus = status; });
	}

	private void SetValueCrossThread(Action action)
	{
		_dispatcherQueue.TryEnqueue(() => { action(); });
	}

	private void OnTaskStart()
	{
		_cts = new CancellationTokenSource();
		AnalysisStarted = true;
		AnalysisBtnStatus = "取消任务";
	}

	private void OnTaskFinished()
	{
		_cts.Dispose();
		_cts = null;
		AnalysisBtnStatus = "开始执行";

		AnalysisStarted = false;
	}

	private void CancelTask()
	{
		_cts?.Cancel();
	}

	#endregion

	[RelayCommand]
	private async Task AnalysisPressedAsync(object canvas)
	{
		if (AnalysisStarted) CancelTask();
		else OnTaskStart();

		var startTime = DateTime.Now;

		if (TaskDetail.StartDate > TaskDetail.EndDate)
		{
			SetCurrentStatus("开始日期不能小于结束日");
			(TaskDetail.StartDate, TaskDetail.EndDate) = (TaskDetail.EndDate, TaskDetail.StartDate);
			OnTaskFinished();
			return;
		}

		if (TaskDetail.PullUpBeforeStart > TaskDetail.PullUpBeforeEnd)
		{
			SetCurrentStatus("前期涨幅设置错误");
			(TaskDetail.PullUpBeforeStart, TaskDetail.PullUpBeforeEnd) = (TaskDetail.PullUpBeforeEnd, TaskDetail.PullUpBeforeStart);
			OnTaskFinished();
			return;
		}

		var stocks = new List<Stock>();

		SetCurrentStatus("格式化输入。。");

		//store last queried codes
		var codes = StringHelper.FormatInputCode(CodeInput);

		if (codes == null || !codes.Any())
		{
			SetCurrentStatus("输入为空");
			OnTaskFinished();
			return;
		}

		var acStocks = _fetchStockDataService.GenerateStocks(codes);
		await foreach (var stock in acStocks) stocks.Add(stock);

		SetCurrentStatus("保存历史查询数据。。。");

		// store this activity
		await Task.Run(async () => { await _dbTransferService.StoreActivityDataToDb(DateTime.Now, CodeInput, string.Empty); });

		var config = new StatisticsConfiguration
		{
			TaskDetail = TaskDetail,
			Stocks = stocks
		};

		config.OnAnalyzeCallback += NotifyUserCurrentStatus;

		try
		{
			SetCurrentStatus("开始。。。");

			var report = await _stockAnalyzeService.Statistics(config, _cts.Token);

			_chartService.DrawStatisticsResultA(canvas as Canvas, report);
		}
		catch (TaskCanceledException e)
		{
			SetCurrentStatus("已取消。。。");
			Console.WriteLine(e);
			Console.WriteLine(GetType());
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Console.WriteLine(GetType());
		}
		finally
		{
			OnTaskFinished();
		}

		await Task.Delay(1000);
		var endTime = DateTime.Now;
		AnalysisStarted = false;
		SetCurrentStatus("用时：" + (endTime - startTime).Seconds + "秒");
	}

	private void NotifyUserCurrentStatus(object obj, NotifyUserEventArgs e)
	{
		SetCurrentStatus(e.Message);
	}

	public async Task FormatCodeInput(string input)
	{
		var list = await _fetchStockDataService.FormatInputCode(input);
		CodeInput = StockService.FormatPyArgument(list);
	}

	public void InitializeField()
	{
		TaskDetail.ExpectedProfit    = 10d;
		TaskDetail.PullUpBeforeEnd   = 200d;
		TaskDetail.PullUpBeforeStart = 50d;
		TaskDetail.SelectedYAxis     = 0;
		TaskDetail.StepLength        = 5;
		TaskDetail.StartDate         = DateTimeOffset.Now.AddHours(23 - DateTimeOffset.Now.Hour).AddDays(-375);
		TaskDetail.EndDate           = DateTimeOffset.Now.AddHours(23 - DateTimeOffset.Now.Hour).AddDays(-10);

		TaskDetail.TaskName = "上涨N%后继续上涨M%的历史数据统计";
		TaskDetail.TaskDetail = "在上涨了X%之后还会继续上涨N%,最大回撤为B%,持续时间为T天";

		AnalysisBtnStatus = "开始执行";
	}

	public async Task RestoreLastQueryStringAsync()
	{
		// get service
		var restoreSettingsService = App.GetService<IRestoreSettingsService>();
		// query from db
		var lastQuery = await restoreSettingsService.RestoreLastQueryStringFromDb();

		if (lastQuery != null) CodeInput = lastQuery;
	}
}