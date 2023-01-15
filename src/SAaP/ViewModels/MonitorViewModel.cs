using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Contracts.Services;
using System.Collections.ObjectModel;
using System.Text;
using Mapster;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using SAaP.Models;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using Microsoft.UI.Xaml.Controls;
using SAaP.Core.Services.Analyze;
using SAaP.Views;
using SAaP.Core.Models;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Monitor;

namespace SAaP.ViewModels;

public class MonitorViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly IDbTransferService _dbTransferService;
    private readonly IStockAnalyzeService _stockAnalyzeService;
    private readonly IFetchStockDataService _fetchStockDataService;
    private readonly IRestoreSettingsService _restoreSettingsService;
    private readonly IMonitorService _monitorService;

    private readonly Stock _noFoundStock = new() { CompanyName = "找不到对象", CodeName = ">_<0" };

    public readonly IList<double> DefaultStopProfitList = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private string _titleBarMessage;
    private string _currentStatus;

    public ObservableCollection<Stock> AllSuggestStocks { get; } = new();

    public ObservableCollection<Stock> MonitorStocks { get; } = new();

    public ObservableCollection<ObservableTrackCondition> FilterConditions { get; } = new();

    public ObservableCollection<MonitorNotification> SimulationResultCollection { get; } = new();

    public ObservableCollection<ObservableTrackCondition> MonitorConditions { get; } = new();

    public ObservableTrackCondition CurrentTrackFilterCondition { get; set; } = new();

    public ObservableCollection<ObservableTaskDetail> FilterTasks { get; set; } = new();

    public ObservableCollection<MonitorCondition> MonitorTasks { get; set; } = new();

    public ObservableTaskDetail CurrentTaskFilterData { get; set; } = new();

    public MonitorCondition CurrentMonitorData { get; set; } = new();

    public HistoryDeduceData HistoryDeduceData { get; set; } = new();

    public IRelayCommand<object> AddToMonitorCommand { get; }
    public IRelayCommand CheckUseabilityCommand { get; }
    public IRelayCommand SaveFilterTaskCommand { get; }
    public IAsyncRelayCommand AddOnHoldStockCommand { get; }
    public IAsyncRelayCommand SaveFilterConditionCommand { get; }
    public IAsyncRelayCommand HistoryMinDataImportCommand { get; }

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

    public IAsyncRelayCommand OnSimulationStartCommand { get; }

    public MonitorViewModel(IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService, IStockAnalyzeService stockAnalyzeService, IRestoreSettingsService restoreSettingsService, IMonitorService monitorService)
    {
        _dbTransferService = dbTransferService;
        _fetchStockDataService = fetchStockDataService;
        _stockAnalyzeService = stockAnalyzeService;
        _restoreSettingsService = restoreSettingsService;
        _monitorService = monitorService;

        AddToMonitorCommand = new RelayCommand<object>(AddToMonitor);
        AddOnHoldStockCommand = new AsyncRelayCommand(AddOnHoldStock);
        CheckUseabilityCommand = new RelayCommand(CheckUseability);
        SaveFilterConditionCommand = new AsyncRelayCommand(SaveFilterCondition);
        SaveFilterTaskCommand = new RelayCommand(SaveFilterTask);
        HistoryMinDataImportCommand = new AsyncRelayCommand(ImportHistoryMinData);
        OnSimulationStartCommand = new AsyncRelayCommand(OnSimulationStart);
    }

    private async Task OnSimulationStart()
    {
        if (HistoryDeduceData.PreLoadDateStart > HistoryDeduceData.PerLoadDateEnd
            || HistoryDeduceData.PerLoadDateEnd > HistoryDeduceData.AnalyzeEndDate)
        {
            return;
        }

        SimulationResultCollection.Clear();

        HistoryDeduceData.MonitorCondition = CurrentMonitorData.Adapt<MonitorCondition>();

        foreach (var monitorStock in HistoryDeduceData.MonitorStocks)
        {

            var minutesData = _monitorService.ReadMinuteDate(monitorStock, HistoryDeduceData);

            var datas = new List<MinuteData>();

            await foreach (var data in minutesData)
            {
                datas.Add(data);
            }

            if (!datas.Any()) continue;

            await Task.Run(() =>
            {
                var report = _monitorService.StartDeduce(monitorStock, HistoryDeduceData, datas);

                ReportCallback(report);
            });
        }
    }

    private void ReportCallback(MonitorReport report)
    {
        foreach (var notification in report.Notifications)
        {
            SetValueCrossThread(() => { SimulationResultCollection.Add(notification); });
        }
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
        {
            if (ObservableTrackCondition.SelectedTrackIndex.Contains(filterCondition.TrackIndex))
            {
                ready.TrackConditions.Add(filterCondition.HardCopyNew());
            }
        }

        ready.TaskStartEventHandler += OnTaskStartEventHandler;
        ready.NavigateToTabViewEventHandler += OnNavigateToTabViewEventHandler;

        FilterTasks.Insert(0, ready);
    }

    private void SetValueCrossThread(Action action)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            action();
        });
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

        var allCount = AllSuggestStocks.Count;
        var curIndex = 1;
        var selected = 0;

        SetValueCrossThread(() =>
        {
            targetTaskObject.ExecStatus = "开始";
        });

        try
        {
            await Task.Run(async () =>
             {
                 var filtered =
                     _stockAnalyzeService.Filter(
                         AllSuggestStocks
                         , targetTaskObject.TrackConditions
                         , e.CancellationToken);

                 await foreach (var filteredStock in filtered)
                 {
                     // 筛选结果
                     if (filteredStock != null)
                     {
                         selected++;
                         SetValueCrossThread(() =>
                         {
                             targetTaskObject.FilteredStock.Add(filteredStock);
                         });
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

        await foreach (var stock in onHold)
        {
            AddToMonitor(stock);
        }
    }

    public void AddToMonitor(object obj)
    {
        if (obj is not Stock stock) return;
        if (stock == _noFoundStock) return;

        if (MonitorStocks.Any(s => s.CodeNameFull == stock.CodeNameFull)) return;

        MonitorStocks.Add(stock);
        ReinsertToDb(ActivityData.RealTimeMonitor, MonitorStocks);
    }

    public void ReinsertToDb(string type, ObservableCollection<Stock> stocks)
    {
        var sb = new StringBuilder();

        foreach (var code in stocks.Select(a => a.CodeNameFull))
        {
            sb.Append(code).Append(",");
        }

        if (sb.Length < 2) return;

        sb.Remove(sb.Length - 1, 1);

        _dbTransferService.DeleteActivityByAnalyzeData(type);
        _dbTransferService.StoreActivityDataToDb(DateTime.Now, sb.ToString(), type);
    }

    public async void AddToMonitor(string code)
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

        AddToMonitor(stock);
    }

    public async Task InitializeSuggestData()
    {
        var stocks = _dbTransferService.SelectAllLocalStoredCodes();

        await foreach (var stock in stocks)
        {
            AllSuggestStocks.Add(stock);
        }
    }

    public async Task InitializeTrackData()
    {
        var trackDatas = _dbTransferService.SelectTrackData();

        FilterConditions.Clear();
        MonitorConditions.Clear();
        CurrentTrackFilterCondition.Clear();

        await foreach (var trackData in trackDatas)
        {
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
    }

    public async Task InitializeMonitorStockData()
    {
        var realtimeStocks = _restoreSettingsService.RestoreRecentlyActivityListByAnalyzeData(ActivityData.RealTimeMonitor);

        var historyDeduce = _restoreSettingsService.RestoreRecentlyActivityListByAnalyzeData(ActivityData.HistoryDeduce);

        await foreach (var realtimeStock in realtimeStocks)
        {
            MonitorStocks.Add(realtimeStock);
        }

        await foreach (var realtimeStock in historyDeduce)
        {
            HistoryDeduceData.MonitorStocks.Add(realtimeStock);
        }
    }

    public ObservableCollection<Stock> GetCodeSelectSuggest(string[] splitText)
    {
        var itemSource = new ObservableCollection<Stock>();

        foreach (var suggestStock in AllSuggestStocks)
        {
            var found = splitText.All(key => suggestStock.CodeName.Contains(key) || suggestStock.CompanyName.Contains(key));

            if (found)
            {
                itemSource.Add(suggestStock);
            }
        }

        if (!itemSource.Any())
        {
            itemSource.Add(_noFoundStock);
        }

        return itemSource;
    }

    public void DeleteMonitorItem(object dataContext)
    {
        if (dataContext is not Stock stock) return;

        MonitorStocks.Remove(stock);

        ReinsertToDb(ActivityData.RealTimeMonitor, MonitorStocks);
    }

    public void DeleteHistoryDeduce(object dataContext)
    {
        if (dataContext is not Stock stock) return;

        HistoryDeduceData.MonitorStocks.Remove(stock);

        ReinsertToDb(ActivityData.HistoryDeduce, HistoryDeduceData.MonitorStocks);
    }
}