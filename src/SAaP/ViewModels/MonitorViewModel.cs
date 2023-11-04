using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapster;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;
using SAaP.Core.Contracts.Services;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Analyze;
using SAaP.Core.Services.Generic;
using SAaP.Models;
using SAaP.Views;

namespace SAaP.ViewModels;

public class MonitorViewModel : ObservableRecipient
{
	public async Task RestoreSavedTasks()
	{
		try
		{
			var taskSerialized = await _fileService.ReadAsync(StartupService.FilterPath, StartupService.TaskFile);
			if (!string.IsNullOrEmpty(taskSerialized))
			{
				var tasks = await Json.ToObjectAsync<ObservableCollection<ObservableTaskDetail>>(taskSerialized);
				if (tasks.Any())
					foreach (var task in tasks)
					{
						task.TaskStartEventHandler += OnTaskStartEventHandler;
						task.NavigateToTabViewEventHandler += OnNavigateToTabViewEventHandler;
						FilterTasks.Add(task);
					}
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
		}
	}

	private async Task SaveFilterTaskToFile()
	{
		try
		{
			if (FilterTasks.Any())
			{
				//ViewModel.FilterTasks
				await _fileService.SaveAsync(StartupService.FilterPath, StartupService.TaskFile, FilterTasks);
				SaveFilterTaskContent = "已保存!";
			}
			else
			{
				SaveFilterTaskContent = "无任务!";
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
			SaveFilterTaskContent = "出错了!";
		}

		await Task.Delay(1000);
		SaveFilterTaskContent = SaveFilterTaskContentDefault;
	}

	private void ClearSimulateCodes()
	{
		HistoryDeduceData.MonitorStocks.Clear();
	}

	private void AddToSimulate(IList<object> obj)
	{
		if (obj == null || !obj.Any()) return;

		for (var index = 0; index < obj.Count; index++)
		{
			var stock = (Stock)obj[index];
			if (!HistoryDeduceData.MonitorStocks.Contains(stock))
				HistoryDeduceData.MonitorStocks.Add(stock);
		}
	}

	public async Task LiveTask()
	{
		while (true)
		{
			try
			{
				SetValueCrossThread(
					() => { CurrentDateTime = DateTime.Now; }
				);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);Console.WriteLine(GetType());
			}

			await Task.Delay(1000);
		}
		// ReSharper disable once FunctionNeverReturns
	}

	private async Task RealtimeMonitorStart()
	{
		var mealtimes = Time.GetTradeWallTime();

		while (Time.GetTimeRightNow() > mealtimes[3])
		{
			// 盘后
			LoggingCollection.Insert(0, MonitorNotification.SystemNotification("下班了"));

			return;
		}

		bool pqStaff = false, noonStaff = false;

		while (Time.GetTimeRightNow() > mealtimes[1] && Time.GetTimeRightNow() < mealtimes[2])
		{
			// 午休
			if (!noonStaff)
			{
				LoggingCollection.Insert(0, MonitorNotification.SystemNotification("午休"));
				noonStaff = true;
			}

			await Task.Delay(1000);
		}

		while (Time.GetTimeRightNow() < mealtimes[0])
		{
			// 盘前
			if (!pqStaff)
			{
				LoggingCollection.Insert(0, MonitorNotification.SystemNotification("准备中。。。不要关掉窗口啊！！"));
				pqStaff = true;
			}

			await Task.Delay(1000);
		}

		// 盘中
		foreach (var stock in MonitorStocks)
		{
			var dayData = await DbService.TakeOriginalDataFromFile(stock.CodeName, stock.BelongTo, 10);

			if (dayData == null) continue;

			var oldDataStartAt = dayData.Count > 0 ? DateTime.Parse(dayData.First().Day) : DateTime.Now;

			var minutesData =
				await _monitorService.ReadMinuteDateSince(stock, CurrentMonitorData.MinuteType, oldDataStartAt);

			var stock1 = stock;
			await Task.Run(() =>
				{
					// run 里不要用async await！
					_monitorService.RealTimeTrack(stock1, CurrentMonitorData, minutesData,
												  notification => { SetValueCrossThread(() => { PrintRealTimeTrackResult(notification); }); });
				}
			);
		}

		while (Time.GetTimeRightNow() > mealtimes[0] && Time.GetTimeRightNow() < mealtimes[3])
		{
			// 上班时间！

			while (Time.GetTimeRightNow() > mealtimes[1] && Time.GetTimeRightNow() < mealtimes[2])
			{
				// 午休
				if (!noonStaff)
				{
					LoggingCollection.Insert(0, MonitorNotification.SystemNotification("午休"));

					noonStaff = true;
				}

				await Task.Delay(1000);
			}

			await Task.Delay(60000);
		}

		// log in 15:00
		await App.Logger.Log(RealtimeResultCollection.Select(n => n.ToString()).ToList());
		await App.Logger.Log(LoggingCollection.Select(n => n.ToString()).ToList());
	}

	private void PrintRealTimeTrackResult(MonitorNotification notification)
	{
		if (string.IsNullOrEmpty(notification.CodeName))
		{
			LoggingCollection.Insert(0, MonitorNotification.SystemNotification(notification.Message));
		}
		else
		{
			RealtimeResultCollection.Add(notification);
			LoggingCollection.Insert(0, notification);
		}
	}

	private async Task DeleteMonitorItems(IList<object> selectedItems)
	{
		if (!selectedItems.Any()) return;

		var codeNames = (from Stock stock in selectedItems select stock.CodeNameFull).ToList();

		foreach (var first in codeNames.Select(codeName => MonitorStocks.First(s => s.CodeNameFull == codeName)))
			MonitorStocks.Remove(first);

		await ReinsertToDb(ActivityData.RealTimeMonitor, MonitorStocks);
	}

	private async Task ImportStockFromText()
	{
		if (string.IsNullOrEmpty(ImportStockText)) return;

		var codes = await _fetchStockDataService.FormatInputCode(ImportStockText);

		foreach (var stock in
				 from code in codes
				 select AllSuggestStocks.Where(s => s.CodeNameFull == code).ToList()
				 into stocks
				 where stocks.Any()
				 select stocks.First()
				 into stock
				 where !MonitorStocks.Contains(stock)
				 select stock)
			MonitorStocks.Add(stock);

		ImportStockText = string.Empty;

		await ReinsertToDb(ActivityData.RealTimeMonitor, MonitorStocks);
	}

	private async Task OnSimulationStart()
	{
		if (HistoryDeduceData.PreLoadDateStart > HistoryDeduceData.PerLoadDateEnd
			|| HistoryDeduceData.PerLoadDateEnd > HistoryDeduceData.AnalyzeEndDate)
			return;

		SimulationResultCollection.Clear();

		HistoryDeduceData.MonitorCondition = CurrentMonitorData.Adapt<MonitorCondition>();

		var monitorStocks = SimulateAllMonitorCode ? MonitorStocks : HistoryDeduceData.MonitorStocks;

		foreach (var monitorStock in monitorStocks)
		{
			var minutesData = _monitorService.ReadMinuteDateForSimulate(monitorStock, HistoryDeduceData);

			var datas = new List<MinuteData>();

			await foreach (var data in minutesData) datas.Add(data);

			if (!datas.Any())
			{
				SetValueCrossThread(() =>
				{
					LoggingCollection.Insert(0,
											 new MonitorNotification
											 {
												 CodeName = monitorStock.CodeName,
												 CompanyName = monitorStock.CompanyName,
												 FullTime = Time.GetTimeOffsetRightNow(),
												 Message = "未导入历史数据/未上市/已退市/休市"
											 });
				});
				continue;
			}

			await Task.Run(() =>
			{
				var report = _monitorService.StartDeduce(monitorStock, HistoryDeduceData, datas);

				if (!report.Notifications.Any() && !SimulateAllMonitorCode)
				{
					SetValueCrossThread(() =>
					{
						LoggingCollection.Insert(0,
												 new MonitorNotification
												 {
													 CodeName = monitorStock.CodeName,
													 CompanyName = monitorStock.CompanyName,
													 FullTime = Time.GetTimeOffsetRightNow(),
													 Message = "无结果"
												 });
					});
					return;
				}


				ReportCallback(report);
			});
		}

#if DEBUG
		await App.Logger.Log(SimulationResultCollection.Select(n => n.ToString()).ToList());
		await App.Logger.Log(LoggingCollection.Select(n => n.ToString()).ToList());

#endif
	}

	private void ReportCallback(MonitorReport report)
	{
		foreach (var notification in report.Notifications)
			SetValueCrossThread(() => { SimulationResultCollection.Add(notification); });
	}

	private async Task ImportHistoryMinData()
	{
		if (!MonitorStocks.Any()) return;

		var pyArgs = StockService.FormatPyArgument(MonitorStocks.Select(m => m.CodeNameFull));

		CurrentStatus = "正在导入1分钟线数据";
		await _fetchStockDataService.FetchStockMinuteData(pyArgs, 1);

		CurrentStatus = "正在导入5分钟线数据";
		await _fetchStockDataService.FetchStockMinuteData(pyArgs, 5);

		CurrentStatus = string.Empty;
	}

	private void SaveFilterTask()
	{
		var ready = CurrentTaskFilterData.Adapt<ObservableTaskDetail>();

		foreach (var filterCondition in FilterConditions)
			if (ObservableTrackCondition.SelectedTrackIndex.Contains(filterCondition.TrackIndex))
				ready.TrackConditions.Add(filterCondition.HardCopyNew());

		if (!ready.TrackConditions.Any()) return;

		var details = FilterTasks.Select(t => t.TaskDetail).ToList();

		if (details.Contains(ready.TaskDetail)) return;

		ready.TaskStartEventHandler += OnTaskStartEventHandler;
		ready.NavigateToTabViewEventHandler += OnNavigateToTabViewEventHandler;
		FilterTasks.Insert(0, ready);
	}

	private void SetValueCrossThread(Action action)
	{
		_dispatcherQueue.TryEnqueue(() => { action(); });
	}

	private static TabViewItem CreateNewTab(Type target, NavigateToTabViewEventArgs e)
	{
		var newItem = new TabViewItem
		{
			Header = e.TaskName,
			IconSource = new SymbolIconSource { Symbol = Symbol.Filter }
		};

		var frame = new Frame();

		frame.Navigate(target, e.Stocks);

		newItem.Content = frame;

		return newItem;
	}

	private static void OnNavigateToTabViewEventHandler(object sender, NavigateToTabViewEventArgs e)
	{
		var tabView = e.Target;
		var newTab = CreateNewTab(typeof(FilterResultViewerPage), e);
		tabView.TabItems.Add(newTab);

		tabView.SelectedItem = newTab;
	}

	private async void OnTaskStartEventHandler(object sender, TaskStartEventArgs e)
	{
		if (sender is not ObservableTaskDetail targetTaskObject) return;

		var filterFromStocks = AllSuggestStocks.ToList();

		//BuySimulateMode

		var allCount = filterFromStocks.Count;
		var curIndex = 1;
		var selected = 0;

		SetValueCrossThread(() => { targetTaskObject.ExecStatus = "开始"; });

		try
		{
			await Task.Run(async () =>
			{
				var filtered =
					_stockAnalyzeService.Filter(
						filterFromStocks
						, targetTaskObject.TrackConditions
						, CustomLastTradingDate
						, e.CancellationToken);

				await foreach (var filteredStock in filtered)
				{
					// 筛选结果
					if (filteredStock != null)
					{
						selected++;
						SetValueCrossThread(() => { targetTaskObject.FilteredStock.Add(filteredStock); });
					}

					var index = curIndex;
					var selected1 = selected;
					SetValueCrossThread(() =>
						{
							// ReSharper disable once PossibleLossOfFraction
							targetTaskObject.ProgressBarValue = 100 * index / allCount;
							targetTaskObject.ExecProgress = $"筛选结果:{selected1}/{allCount}";
						}
					);
					curIndex++;
				}
			}, e.CancellationToken);
		}
		catch (TaskCanceledException)
		{
			SetValueCrossThread(() =>
			{
				targetTaskObject!.ExecStatus = "已取消！";

				targetTaskObject.OnTaskCancelled();
			});

			return;
		}

		SetValueCrossThread(() =>
		{
			targetTaskObject!.ExecStatus = "完成！";

			targetTaskObject.OnTaskComplete();
		});
	}

	public async Task DeleteFilterTrackData(object data)
	{
		var dbTransferService = App.GetService<IDbTransferService>();

		await dbTransferService.DeleteTrackData(data.Adapt<TrackData>());
		await InitializeTrackData();
	}

	public void EditFilterTrackData(object data)
	{
		if (data is not ObservableTrackCondition trackData) return;

		CurrentTrackFilterCondition.TrackIndex = trackData.TrackIndex;
		CopyFilterTrackData(data);
	}

	public void CopyFilterTrackData(object data)
	{
		if (data is not ObservableTrackCondition trackData) return;

		CurrentTrackFilterCondition.TrackIndex = ObservableTrackCondition.DefaultTrackIndex;
		CurrentTrackFilterCondition.TrackContent = trackData.TrackContent;
		CurrentTrackFilterCondition.TrackSummary = trackData.TrackSummary;
		CurrentTrackFilterCondition.IsValid = true;
		CurrentTrackFilterCondition.TrackName = trackData.TrackName;
	}

	public void DeleteFilterTask(object data)
	{
		if (data is not ObservableTaskDetail taskDetail) return;

		FilterTasks.Remove(taskDetail);
	}

	private async Task SaveFilterCondition()
	{
		await _dbTransferService.InsertTrackData(CurrentTrackFilterCondition.Adapt<TrackData>());

		await InitializeTrackData();
	}

	private void CheckUseability()
	{
		var content = CurrentTrackFilterCondition.TrackContent;
		CurrentTrackFilterCondition.IsValid = Condition.TryParse(ref content);
		CurrentTrackFilterCondition.TrackContent = content;
	}

	private async Task AddOnHoldStock()
	{
		var onHold = _dbTransferService.SelectStockOnHold();

		await foreach (var stock in onHold) await AddToMonitor(stock);
	}

	public async Task AddToMonitor(object obj)
	{
		if (obj is not Stock stock) return;
		if (stock == _noFoundStock) return;

		if (MonitorStocks.Any(s => s.CodeNameFull == stock.CodeNameFull)) return;

		MonitorStocks.Add(stock);
		await ReinsertToDb(ActivityData.RealTimeMonitor, MonitorStocks);
	}

	public async Task ReinsertToDb(string type, ObservableCollection<Stock> stocks)
	{
		if (!stocks.Any())
		{
			await _dbTransferService.DeleteActivityByAnalyzeData(type);
			return;
		}

		var sb = new StringBuilder();

		foreach (var code in stocks.Select(a => a.CodeNameFull)) sb.Append(code).Append(",");

		if (sb.Length < 2) return;

		sb.Remove(sb.Length - 1, 1);

		await _dbTransferService.DeleteActivityByAnalyzeData(type);
		await _dbTransferService.StoreActivityDataToDb(DateTime.Now, sb.ToString(), type);
	}

	public async Task AddToMonitor(string code)
	{
		if (string.IsNullOrEmpty(code)) return;

		var belongTo = await _fetchStockDataService.TryGetBelongByCode(code);

		if (belongTo < 0) return;

		var stock = new Stock
		{
			CodeName = code,
			BelongTo = belongTo,
			CompanyName = await DbService.SelectCompanyNameByCode(code, belongTo)
		};

		await AddToMonitor(stock);
	}

	public async Task InitializeSuggestData()
	{
		if (AllSuggestStocks.Any()) return;

		var stocks = _dbTransferService.SelectAllLocalStoredCodes();

		await foreach (var stock in stocks) AllSuggestStocks.Add(stock);
	}

	public async Task InitializeTrackData()
	{
		var trackDatas = _dbTransferService.SelectTrackData();

		FilterConditions.Clear();
		MonitorConditions.Clear();
		CurrentTrackFilterCondition.Clear();

		await foreach (var trackData in trackDatas)
			switch (trackData.TrackType)
			{
				case TrackType.Filter:
					FilterConditions.Add(trackData.Adapt<ObservableTrackCondition>());
					break;
				case TrackType.Monitor:
					MonitorConditions.Add(trackData.Adapt<ObservableTrackCondition>());
					break;
				case TrackType.Unknown:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
	}

	public async Task InitializeMonitorStockData()
	{
		var realtimeStocks =
			_restoreSettingsService.RestoreRecentlyActivityListByAnalyzeData(ActivityData.RealTimeMonitor);

		var historyDeduce =
			_restoreSettingsService.RestoreRecentlyActivityListByAnalyzeData(ActivityData.HistoryDeduce);

		await foreach (var realtimeStock in realtimeStocks) MonitorStocks.Add(realtimeStock);

		await foreach (var realtimeStock in historyDeduce) HistoryDeduceData.MonitorStocks.Add(realtimeStock);
	}

	public ObservableCollection<Stock> GetCodeSelectSuggest(string[] splitText)
	{
		var itemSource = new ObservableCollection<Stock>();

		foreach (var suggestStock in AllSuggestStocks)
		{
			var found = splitText.All(key =>
										  suggestStock.CodeName.Contains(key) || suggestStock.CompanyName.Contains(key));

			if (found) itemSource.Add(suggestStock);
		}

		if (!itemSource.Any()) itemSource.Add(_noFoundStock);

		return itemSource;
	}

	public void DeleteMonitorItem(object dataContext)
	{
		if (dataContext is not Stock stock) return;

		var s = MonitorStocks.First(s => s.CodeNameFull == stock.CodeNameFull);

		MonitorStocks.Remove(s);
	}

	public async Task DeleteHistoryDeduce(object dataContext)
	{
		if (dataContext is not Stock stock) return;

		HistoryDeduceData.MonitorStocks.Remove(stock);

		await ReinsertToDb(ActivityData.HistoryDeduce, HistoryDeduceData.MonitorStocks);
	}

	#region VIEWMODEL FIELD

	// ReSharper disable once CollectionNeverQueried.Global
	private const string SaveFilterTaskContentDefault = "保存所有任务";
	private readonly Stock _noFoundStock = new() { CompanyName = "找不到对象", CodeName = ">_<0" };
	public readonly IList<double> DefaultStopProfitList = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
	private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
	private readonly IFetchStockDataService _fetchStockDataService;
	private readonly IRestoreSettingsService _restoreSettingsService;
	private readonly IStockAnalyzeService _stockAnalyzeService;
	private readonly IDbTransferService _dbTransferService;
	private readonly IMonitorService _monitorService;
	private readonly IFileService _fileService;

	private string _currentStatus;
	private string _importStockText;
	private string _titleBarMessage;
	private string _saveFilterTaskContent;
	private bool _buySimulateMode;
	private bool _simulateAllMonitorCode;
	private double _taskHeight;
	private DateTime _currentDateTime;
	private DateTimeOffset _customLastTradingDate;


	public MonitorViewModel(IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService,
							IStockAnalyzeService stockAnalyzeService, IRestoreSettingsService restoreSettingsService,
							IMonitorService monitorService, IFileService fileService)
	{
		_dbTransferService = dbTransferService;
		_fetchStockDataService = fetchStockDataService;
		_stockAnalyzeService = stockAnalyzeService;
		_restoreSettingsService = restoreSettingsService;
		_monitorService = monitorService;
		_fileService = fileService;

		CustomLastTradingDate = DateTimeOffset.Now;
		SaveFilterTaskContent = SaveFilterTaskContentDefault;

		AddToMonitorCommand = new AsyncRelayCommand<object>(AddToMonitor);
		DeleteMonitorItemsCommand = new AsyncRelayCommand<IList<object>>(DeleteMonitorItems);
		AddOnHoldStockCommand = new AsyncRelayCommand(AddOnHoldStock);
		SaveFilterConditionCommand = new AsyncRelayCommand(SaveFilterCondition);
		HistoryMinDataImportCommand = new AsyncRelayCommand(ImportHistoryMinData);
		OnSimulationStartCommand = new AsyncRelayCommand(OnSimulationStart);
		ImportStockFromTextCommand = new AsyncRelayCommand(ImportStockFromText);
		RealtimeMonitorStartCommand = new AsyncRelayCommand(RealtimeMonitorStart);
		SaveFilterTaskToFileCommand = new AsyncRelayCommand(SaveFilterTaskToFile);
		CheckUseabilityCommand = new RelayCommand(CheckUseability);
		SaveFilterTaskCommand = new RelayCommand(SaveFilterTask);
		ClearSimulateCodesCommand = new RelayCommand(ClearSimulateCodes);
		AddToSimulateCommand = new RelayCommand<IList<object>>(AddToSimulate);
	}

	public static ObservableCollection<Stock> AllSuggestStocks { get; } = new();

	public ObservableCollection<Stock> MonitorStocks { get; } = new();

	public ObservableCollection<ObservableTrackCondition> FilterConditions { get; } = new();

	public ObservableCollection<MonitorNotification> RealtimeResultCollection { get; } = new();

	public ObservableCollection<MonitorNotification> SimulationResultCollection { get; } = new();

	public ObservableCollection<MonitorNotification> LoggingCollection { get; } = new();

	public ObservableCollection<ObservableTrackCondition> MonitorConditions { get; } = new();

	public ObservableTrackCondition CurrentTrackFilterCondition { get; set; } = new();

	public ObservableCollection<ObservableTaskDetail> FilterTasks { get; set; } = new();

	public ObservableCollection<MonitorCondition> MonitorTasks { get; set; } = new();

	public ObservableTaskDetail CurrentTaskFilterData { get; set; } = new();

	public MonitorCondition CurrentMonitorData { get; set; } = new();

	public HistoryDeduceData HistoryDeduceData { get; set; } = new();

	public IAsyncRelayCommand<object> AddToMonitorCommand { get; }
	public IRelayCommand<IList<object>> AddToSimulateCommand { get; }
	public IRelayCommand ClearSimulateCodesCommand { get; }
	public IRelayCommand CheckUseabilityCommand { get; }
	public IRelayCommand SaveFilterTaskCommand { get; }
	public IAsyncRelayCommand AddOnHoldStockCommand { get; }
	public IAsyncRelayCommand SaveFilterConditionCommand { get; }
	public IAsyncRelayCommand HistoryMinDataImportCommand { get; }
	public IAsyncRelayCommand OnSimulationStartCommand { get; }
	public IAsyncRelayCommand ImportStockFromTextCommand { get; }
	public IAsyncRelayCommand<IList<object>> DeleteMonitorItemsCommand { get; }
	public IAsyncRelayCommand RealtimeMonitorStartCommand { get; }
	public IAsyncRelayCommand SaveFilterTaskToFileCommand { get; }

	public string TitleBarMessage
	{
		get => _titleBarMessage;
		set => SetProperty(ref _titleBarMessage, value);
	}

	public string CurrentStatus
	{
		get => _currentStatus;
		set => SetProperty(ref _currentStatus, value);
	}

	public string ImportStockText
	{
		get => _importStockText;
		set => SetProperty(ref _importStockText, value);
	}

	public DateTime CurrentDateTime
	{
		get => _currentDateTime;
		set => SetProperty(ref _currentDateTime, value);
	}

	public bool SimulateAllMonitorCode
	{
		get => _simulateAllMonitorCode;
		set => SetProperty(ref _simulateAllMonitorCode, value);
	}

	public bool BuySimulateMode
	{
		get => _buySimulateMode;
		set => SetProperty(ref _buySimulateMode, value);
	}

	public DateTimeOffset CustomLastTradingDate
	{
		get => _customLastTradingDate;
		set => SetProperty(ref _customLastTradingDate, value);
	}

	public double TaskHeight
	{
		get => _taskHeight;
		set => SetProperty(ref _taskHeight, value);
	}

	public string SaveFilterTaskContent
	{
		get => _saveFilterTaskContent;
		set => SetProperty(ref _saveFilterTaskContent, value);
	}

	#endregion
}