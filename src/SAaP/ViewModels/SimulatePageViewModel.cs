using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using SAaP.Contracts.Services;
using SAaP.Core.Helpers;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;
using SAaP.Models;

namespace SAaP.ViewModels;

public partial class SimulatePageViewModel : ObservableRecipient
{
	private readonly             IDbTransferService      _dbTransferService;
	private readonly             DispatcherQueue         _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
	private readonly             IFetchStockDataService  _fetchStockDataService;
	private readonly             IStockAnalyzeService    _stockAnalyzeService;
	[ObservableProperty] private string                  _analysisBtnStatus;
	[ObservableProperty] private bool                    _analysisStarted;
	[ObservableProperty] private string                  _codeInput;
	private                      CancellationTokenSource _cts;
	[ObservableProperty] private string                  _currentStatus;
	[ObservableProperty] private DateTimeOffset          _endDate;
	[ObservableProperty] private bool                    _isAnalysisStarted;
	[ObservableProperty] private DateTimeOffset          _startDate;

	public SimulatePageViewModel(IStockAnalyzeService stockAnalyzeService, IFetchStockDataService fetchStockDataService,
	                             IDbTransferService dbTransferService)
	{
		_stockAnalyzeService   = stockAnalyzeService;
		_fetchStockDataService = fetchStockDataService;
		_dbTransferService     = dbTransferService;
	}

	public ObservableTaskDetail TaskDetail { get; set; } = new();

	public ObservableCollection<SimulateResult> SimulateResults { get; set; } = new();

	public ReportSummary ReportSummary { get; set; } = new();

	private void SetValueCrossThread(Action action)
	{
		_dispatcherQueue.TryEnqueue(() => { action(); });
	}

	private void NotifyUserCurrentStatus(object obj, NotifyUserEventArgs e)
	{
		SetCurrentStatus(e.Message);
	}

	private void SetCurrentStatus(string status)
	{
		SetValueCrossThread(() => { CurrentStatus = status; });
	}

	private void OnTaskStart()
	{
		_cts              = new CancellationTokenSource();
		AnalysisStarted   = true;
		AnalysisBtnStatus = "取消任务";
	}

	private void OnTaskFinished()
	{
		_cts.Dispose();
		_cts              = null;
		AnalysisBtnStatus = "开始执行";

		AnalysisStarted = false;
	}

	private void CancelTask()
	{
		_cts?.Cancel();
	}

	[RelayCommand]
	private async Task AnalysisPressedAsync()
	{
		if (AnalysisStarted) CancelTask();
		else OnTaskStart();

		var startTime = DateTime.Now;

		if (StartDate > EndDate)
		{
			SetCurrentStatus("开始日期不能小于结束日");
			(StartDate, EndDate) = (EndDate, StartDate);
			OnTaskFinished();
			return;
		}

		var stocks = new List<Stock>();

		SetCurrentStatus("格式化输入。。");

		//store last queried codes
		var codes = StringHelper.FormatInputCode(CodeInput);

		if (codes != null && codes.Any())
		{
			var acStocks = _fetchStockDataService.GenerateStocks(codes);

			await foreach (var stock in acStocks) stocks.Add(stock);
		}

		SetCurrentStatus("保存历史查询数据。。。");

		// store this activity
		await Task.Run(async () => { await _dbTransferService.StoreActivityDataToDb(DateTime.Now, CodeInput, string.Empty); });

		var config = new SimulateConfiguration
		{
			TaskDetail = TaskDetail,
			StartDate  = StartDate.DateTime,
			EndDate    = EndDate.DateTime,
			Stocks     = stocks
		};

		config.OnAnalyzeCallback += NotifyUserCurrentStatus;

		try
		{
			SetCurrentStatus("开始。。。");

			var report = await _stockAnalyzeService.Simulate(config, _cts.Token);

			if (report != null && report.SimulateResults.Any())
			{
				SimulateResults.Clear();
				foreach (var reportSimulateResult in report.SimulateResults) SimulateResults.Add(reportSimulateResult);

				ReportSummary.Transfer(report.ReportSummary);
			}
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

	public async Task RestoreLastQueryStringAsync()
	{
		// get service
		var restoreSettingsService = App.GetService<IRestoreSettingsService>();
		// query from db
		var lastQuery = await restoreSettingsService.RestoreLastQueryStringFromDb();

		if (lastQuery != null) CodeInput = lastQuery;
	}

	public async Task FormatCodeInput(string input)
	{
		var list = await _fetchStockDataService.FormatInputCode(input);
		CodeInput = StockService.FormatPyArgument(list);
	}

	public void InitializeField()
	{
		StartDate         = DateTimeOffset.Now.AddHours(23 - DateTimeOffset.Now.Hour).AddDays(-150);
		EndDate           = DateTimeOffset.Now.AddHours(23 - DateTimeOffset.Now.Hour).AddDays(-10);
		AnalysisBtnStatus = "开始执行";
	}
}