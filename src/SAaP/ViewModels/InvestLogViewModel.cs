using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Contracts.Services;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using SAaP.Models;

namespace SAaP.ViewModels;

public class InvestLogViewModel : ObservableRecipient
{
    private int _buySelectedIndex;
    private int _sellSelectedIndex;

    private readonly IDbTransferService _dbTransferService;
    private readonly IFetchStockDataService _fetchStockDataService;

    private string _notifyContent;
    private string _newSummaryRecordCodeName;
    private string _newSummaryRecordCompanyName;
    private int _tradeHistorySelectedIndex;
    private int _reminderSelectedIndex;
    private string _reminderContent;

    public ObservableCollection<ObservableInvestSummaryDetail> InvestSummary { get; set; } = new();

    public ObservableCollection<ObservableInvestDetail> BuyList { get; set; } = new();

    public ObservableCollection<ObservableInvestDetail> SellList { get; set; } = new();

    public ObservableInvestSummaryDetail InvestSummaryDetail { get; set; } = new();

    public ObservableCollection<RemindMessageData> RemindMessages { get; set; } = new();

    public IRelayCommand<object> AddNewTradeRecordCommand { get; set; }

    public TradeType ClickedRowsOriginalTradeType { get; set; }

    public int BuySelectedIndex
    {
        get => _buySelectedIndex;
        set => SetProperty(ref _buySelectedIndex, value);
    }

    public int SellSelectedIndex
    {
        get => _sellSelectedIndex;
        set => SetProperty(ref _sellSelectedIndex, value);
    }

    public string NotifyContent
    {
        get => _notifyContent;
        set => SetProperty(ref _notifyContent, value);
    }

    public string NewSummaryRecordCodeName
    {
        get => _newSummaryRecordCodeName;
        set => SetProperty(ref _newSummaryRecordCodeName, value);
    }

    public string NewSummaryRecordCompanyName
    {
        get => _newSummaryRecordCompanyName;
        set => SetProperty(ref _newSummaryRecordCompanyName, value);
    }

    public int TradeHistorySelectedIndex
    {
        get => _tradeHistorySelectedIndex;
        set => SetProperty(ref _tradeHistorySelectedIndex, value);
    }

    public string ReminderContent
    {
        get => _reminderContent;
        set => SetProperty(ref _reminderContent, value);
    }

    public int ReminderSelectedIndex
    {
        get => _reminderSelectedIndex;
        set => SetProperty(ref _reminderSelectedIndex, value);
    }

    public IAsyncRelayCommand SaveRecordCommand { get; }
    public IRelayCommand NewSummaryRecordCommand { get; }


    public InvestLogViewModel
        (IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService)
    {
        _dbTransferService = dbTransferService;
        _fetchStockDataService = fetchStockDataService;

        BuyList.CollectionChanged += OnCollectionChanged;
        SellList.CollectionChanged += OnCollectionChanged;

        AddNewTradeRecordCommand = new RelayCommand<object>(AddNewTradeRecord);
        SaveRecordCommand = new AsyncRelayCommand(SaveRecordAsync);
        NewSummaryRecordCommand = new RelayCommand(NewSummaryRecordAsync);
    }

    private void NewSummaryRecordAsync()
    {
        if (string.IsNullOrEmpty(NewSummaryRecordCodeName))
        {
            SetNotifyContent("股票代码为必填");
        }

        ClearCurrentDisplayedSummaryRecord();

        InvestSummaryDetail.CodeName = NewSummaryRecordCodeName;
        InvestSummaryDetail.CompanyName = string.IsNullOrEmpty(NewSummaryRecordCompanyName) ? "Unknown" : NewSummaryRecordCompanyName;
    }

    public void ClearCurrentDisplayedSummaryRecord()
    {
        InvestSummaryDetail.Clear();
        BuyList.Clear();
        SellList.Clear();
    }

    public async Task InitialInvestSummaryDetail()
    {
        InvestSummary.Clear();

        var summaryData = _dbTransferService.SelectInvestSummaryData();

        await foreach (var s in summaryData)
        {
            InvestSummary.Add(s.Adapt<ObservableInvestSummaryDetail>());
        }
    }

    public async Task InitialFirstSummaryDetail()
    {
        if (InvestSummary.Any())
        {
            await InitialInvestSummaryDetail(InvestSummary[0]);
        }
        else
        {
            InvestSummaryDetail.DefaultArchived();
        }
    }

    public async Task InitialInvestSummaryDetail(ObservableInvestSummaryDetail detail)
    {
        ClearCurrentDisplayedSummaryRecord();

        var newest = detail.TradeIndex;

        detail.Adapt(InvestSummaryDetail);
        InvestSummaryDetail.EnsureArchived();

        var investDatas = _dbTransferService.SelectInvestDataByIndex(newest);

        var edi = !InvestSummaryDetail.IsArchived;

        await foreach (var investData in investDatas)
        {
            var item = investData.Adapt<ObservableInvestDetail>();
            item.Editable = edi;

            TradeRecordBindCallBack(item);

            switch (investData.TradeType)
            {
                case TradeType.Buy:
                case TradeType.Unknown:
                    BuyList.Add(item); break;
                case TradeType.Sell:
                    SellList.Add(item); break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO next 
    }

    private void SetNotifyContent(string content)
    {
        NotifyContent = content;
    }

    private async Task SaveRecordAsync()
    {
        if (string.IsNullOrEmpty(InvestSummaryDetail.CodeName))
        {
            SetNotifyContent("股票代码为控！");
            return;
        }

        // save to InvestSummaryData table
        await _dbTransferService.SaveToInvestSummaryDataToDb(InvestSummaryDetail);

        var list = BuyList.ToList();
        list.AddRange(SellList);

        await _dbTransferService.SaveToInvestDataToDb(InvestSummaryDetail, list);

        if (InvestSummaryDetail.IsArchived)
        {
            InvestSummaryDetail.IsArchivedAndSavedToDb = true;

            foreach (var buy in BuyList)
            {
                buy.Editable = false;
            }
            foreach (var sell in SellList)
            {
                sell.Editable = false;
            }
        }

        await InitialInvestSummaryDetail();
    }

    public bool CheckIfSoldAll()
    {
        var buySum = BuyList.Sum(o => o.Volume);
        var sellSum = SellList.Sum(o => o.Volume);

        return (InvestSummaryDetail.TradeIndex > 0 || sellSum > 0) && buySum == sellSum;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var principle = 0.0;
        var sold = 0.0;
        var bSum = BuyList.Sum(b => b.Volume);
        var sSum = SellList.Sum(b => b.Volume);

        if (BuyList.Any())
        {
            principle = BuyList.Sum(b => b.Price * b.Volume);
            double buyVolume = BuyList.Sum(b => b.Volume);

            InvestSummaryDetail.Start = BuyList.Min(b => b.TradeDate);
            InvestSummaryDetail.AverageCost = CalculationService.Round3(principle / buyVolume);
        }
        if (SellList.Any())
        {
            sold = SellList.Sum(b => b.Price * b.Volume);
            double sellVolume = SellList.Sum(b => b.Volume);

            InvestSummaryDetail.End = SellList.Max(b => b.TradeDate);
            InvestSummaryDetail.AverageSell = CalculationService.Round3(sold / sellVolume);
        }

        // sold all
        if (bSum == sSum)
        {
            InvestSummaryDetail.Volume = bSum;
            InvestSummaryDetail.Profit = CalculationService.Round2(100 * (sold - principle) / principle);
        }
        else
        {
            if (!InvestSummaryDetail.IsArchivedAndSavedToDb)
            {
                InvestSummaryDetail.IsArchived = false;
            }

            InvestSummaryDetail.Volume = sSum;

            var bc = sSum * principle / bSum;
            InvestSummaryDetail.Profit = CalculationService.Round2(100 * (sold - bc) / bc);
        }
    }

    private void DeleteTradeRecord(object observableInvestDetail)
    {
        if (observableInvestDetail is not ObservableInvestDetail investDetail) return;

        switch (investDetail.TradeType)
        {
            case TradeType.Buy:
            case TradeType.Unknown:
                BuyList.RemoveAt(BuySelectedIndex); break;
            case TradeType.Sell:
                SellList.RemoveAt(SellSelectedIndex); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(investDetail.TradeType));
        }

        SortTradeList(BuyList);
        SortTradeList(SellList);
    }

    private static void SortTradeList(ICollection<ObservableInvestDetail> list)
    {
        var ordered = list.OrderBy(detail => detail.TradeDate).ThenBy(detail => TimeSpan.Parse(detail.TradeTime)).ToList();

        list.Clear();

        foreach (var g in ordered)
        {
            list.Add(g);
        }
    }

    private void ModifyTradeRecord(object observableInvestDetail)
    {
        if (observableInvestDetail is not ObservableInvestDetail investDetail) return;

        var modified = investDetail.Adapt<ObservableInvestDetail>();

        switch (ClickedRowsOriginalTradeType)
        {
            case TradeType.Buy:
            case TradeType.Unknown:
                BuyList.RemoveAt(BuySelectedIndex); break;
            case TradeType.Sell:
                SellList.RemoveAt(SellSelectedIndex); break;
            default: throw new ArgumentOutOfRangeException(nameof(modified.TradeType));
        }

        switch (modified.TradeType)
        {
            case TradeType.Buy:
            case TradeType.Unknown:
                BuyList.Add(modified); break;
            case TradeType.Sell:
                SellList.Add(modified); break;
            default: throw new ArgumentOutOfRangeException(nameof(modified.TradeType));
        }

        SortTradeList(BuyList);
        SortTradeList(SellList);
    }

    private void TradeRecordBindCallBack(ObservableInvestDetail detail)
    {
        detail.ConfirmCommand = new RelayCommand<object>(ModifyTradeRecord);
        detail.DeleteCommand = new RelayCommand<object>(DeleteTradeRecord);
    }

    private void AddNewTradeRecord(object observableInvestDetail)
    {
        if (observableInvestDetail is not ObservableInvestDetail investDetail) return;
        {
            var hardCoped = investDetail.Adapt<ObservableInvestDetail>();

            TradeRecordBindCallBack(hardCoped);

            if (hardCoped.TradeType == TradeType.Sell)
            {
                SellList.Add(hardCoped);
                SortTradeList(SellList);
            }
            else
            {
                BuyList.Add(hardCoped);
                SortTradeList(BuyList);
            }
        }
    }

    public async void OnNewSummaryRecordCodeNameFocusOut(object sender, RoutedEventArgs e)
    {
        var box = sender as TextBox;

        if (box == null) return;

        var codeName = box.Text;

        if (string.IsNullOrEmpty(codeName)) return;

        codeName = codeName.Trim();
        var loc = -1;

        switch (codeName.Length)
        {
            case StockService.StandardCodeLength:
                loc = await _fetchStockDataService.TryGetBelongByCode(codeName);
                codeName = loc + codeName;
                NewSummaryRecordCodeName = codeName;
                break;
            case StockService.TdxCodeLength:
                loc = codeName[0];
                break;
        }

        NewSummaryRecordCompanyName = await StockService.FetchCompanyNameByCode(codeName, loc);
    }

    public async Task EditTradeHistory(object dataContext = null)
    {
        ObservableInvestSummaryDetail investSummary;

        if (dataContext == null)
        {
            if (TradeHistorySelectedIndex < 0) return;
            investSummary = InvestSummary[TradeHistorySelectedIndex];
        }
        else
        {
            investSummary = dataContext as ObservableInvestSummaryDetail;
        }

        await InitialInvestSummaryDetail(investSummary);
    }


    public async Task DeleteTradeHistory(object dataContext)
    {
        var summaryDetail = dataContext as ObservableInvestSummaryDetail;

        if (InvestSummary.IndexOf(summaryDetail) < 0) return;
        {
            await _dbTransferService.DeleteInvestSummaryData(summaryDetail);
        }

        InvestSummary.Remove(summaryDetail);
    }

    public async Task DeleteReminder(object dataContext)
    {
        var remindMessageData = dataContext as RemindMessageData;

        if (RemindMessages.IndexOf(remindMessageData) < 0) return;

        await _dbTransferService.DeleteReminder(remindMessageData);

        RemindMessages.Remove(remindMessageData);
    }

    public void EditReminder(object remindMessageData = null)
    {
        var idx = remindMessageData != null ? RemindMessages.IndexOf(remindMessageData as RemindMessageData) : ReminderSelectedIndex;

        if (idx < 0) return;

        ReminderContent = RemindMessages[idx].Message;
    }

    public async Task AddNewReminderCommand(object sender, KeyRoutedEventArgs e)
    {
        var box = sender as TextBox;
        if (box == null) return;

        ReminderContent = box.Text.Trim();
        if (ReminderSelectedIndex < 0)
        {
            await _dbTransferService.AddNewReminder(ReminderContent);
        }
        else
        {
            var upd = RemindMessages[ReminderSelectedIndex];
            upd.Message = ReminderContent;

            await _dbTransferService.UpdateReminder(upd);
        }

        ReminderContent = string.Empty;
        await RefreshReminder();
    }

    public async Task RefreshReminder()
    {
        RemindMessages.Clear();

        var reminders = _dbTransferService.SelectReminder();

        await foreach (var reminder in reminders)
        {
            RemindMessages.Add(reminder);
        }
    }
}