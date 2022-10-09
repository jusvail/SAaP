using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Contracts.Services;
using System.Collections.ObjectModel;
using Mapster;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using SAaP.Models;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace SAaP.ViewModels;

public class MonitorViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly IDbTransferService _dbTransferService;
    private readonly IStockAnalyzeService _stockAnalyzeService;
    private readonly IFetchStockDataService _fetchStockDataService;

    private readonly Stock _noFoundStock = new() { CompanyName = "找不到对象", CodeName = ">_<0" };

    private string _titleBarMessage;

    public ObservableCollection<Stock> AllSuggestStocks { get; } = new();

    public ObservableCollection<Stock> MonitorStocks { get; } = new();

    public ObservableCollection<ObservableTrackCondition> FilterConditions { get; } = new();

    public ObservableCollection<ObservableTrackCondition> MonitorConditions { get; } = new();

    public ObservableTrackCondition CurrentTrackFilterCondition { get; set; } = new();

    public ObservableCollection<ObservableTaskDetail> FilterTasks { get; set; } = new();

    public ObservableTaskDetail CurrentTaskFilterData { get; set; } = new();

    public IRelayCommand<object> AddToMonitorCommand { get; }
    public IRelayCommand CheckUseabilityCommand { get; }
    public IRelayCommand SaveFilterTaskCommand { get; }
    public IAsyncRelayCommand AddOnHoldStockCommand { get; }
    public IAsyncRelayCommand SaveFilterConditionCommand { get; }

    public string TitleBarMessage
    {
        get => _titleBarMessage;
        set => SetProperty(ref _titleBarMessage, value);
    }

    public MonitorViewModel(IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService, IStockAnalyzeService stockAnalyzeService)
    {
        _dbTransferService = dbTransferService;
        _fetchStockDataService = fetchStockDataService;
        _stockAnalyzeService = stockAnalyzeService;

        AddToMonitorCommand = new RelayCommand<object>(AddToMonitor);
        AddOnHoldStockCommand = new AsyncRelayCommand(AddOnHoldStock);
        CheckUseabilityCommand = new RelayCommand(CheckUseability);
        SaveFilterConditionCommand = new AsyncRelayCommand(SaveFilterCondition);
        SaveFilterTaskCommand = new RelayCommand(SaveFilterTask);
    }

    private void SaveFilterTask()
    {
        if (string.IsNullOrEmpty(CurrentTaskFilterData.TaskName)) return;

        var ready = CurrentTaskFilterData.Adapt<ObservableTaskDetail>();

        foreach (var filterCondition in FilterConditions)
        {
            if (ObservableTrackCondition.SelectedTrackIndex.Contains(filterCondition.TrackIndex))
            {
                ready.TrackConditions.Add(filterCondition.HardCopyNew());
            }
        }

        ready.TaskStartEventHandler += ReadyOnTaskStartEventHandler;

        FilterTasks.Insert(0, ready);
    }

    private void SetValueCrossThread(Action action)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            action();
        });
    }

    private async void ReadyOnTaskStartEventHandler(object sender, TaskStartEventArgs e)
    {
        if (sender is not ObservableTaskDetail targetTaskObject) return;

        var allCount = AllSuggestStocks.Count;
        var curIndex = 1;

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
                         AllSuggestStocks.Select(s => s.CodeNameFull)
                         , targetTaskObject.TrackConditions
                         , e.CancellationToken);

                 await foreach (var filteredCodeName in filtered)
                 {
                     var index = curIndex;
                     SetValueCrossThread(() =>
                         {
                             if (!string.IsNullOrEmpty(filteredCodeName))
                             {
                                 targetTaskObject.ExecProgress = $"筛选中：{index}/{allCount}...";
                             }
                             // ReSharper disable once PossibleLossOfFraction
                             targetTaskObject.ProgressBarValue = 100 * index / allCount;
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
            targetTaskObject!.ExecProgress = string.Empty;

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
        CurrentTrackFilterCondition.IsValid = true;
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

        if (MonitorStocks.All(s => s.CodeNameFull != stock.CodeNameFull))
        {
            MonitorStocks.Add(stock);
        }
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
    }
}