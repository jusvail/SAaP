using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Models.DB;
using SAaP.Models;

namespace SAaP.ViewModels;

public class InvestLogViewModel : ObservableRecipient
{
#if DEBUG
    private void Debug()
    {

        BuyList.Add(new ObservableInvestDetail
        {
            Price = 56.0,
            Volume = 2000,
            TradeDate = DateTime.Now,
            TradeType = TradeType.Buy,
            TradeTime = "10:31",
            Editable = true
        });

        BuyList.Add(new ObservableInvestDetail
        {
            Price = 51.0,
            Volume = 122000,
            TradeDate = new DateTime(2021, 12, 2),
            TradeType = TradeType.Buy,
            TradeTime = "14:31",
            Editable = false
        });

        SellList.Add(new ObservableInvestDetail
        {
            Price = 12.0,
            Volume = 2400,
            TradeType = TradeType.Sell,
            TradeDate = new DateTime(2008, 12, 2),
            TradeTime = "9:31",
            Editable = false
        });

        SellList.Add(new ObservableInvestDetail
        {
            Price = 56.0,
            TradeType = TradeType.Sell,
            Volume = 2000,
            TradeDate = DateTime.Now,
            TradeTime = "10:31"
        });

        SellList.Add(new ObservableInvestDetail
        {
            Price = 56.0,
            TradeType = TradeType.Sell,
            Volume = 2000,
            TradeDate = DateTime.Now,
            TradeTime = "10:31"
        });
    }
#endif

    public ObservableCollection<ObservableInvestDetail> TradeList { get; } = new();

    public ObservableCollection<ObservableInvestDetail> BuyList { get; } = new();

    public ObservableCollection<ObservableInvestDetail> SellList { get; } = new();

    public ObservableCollection<InvestSummaryData> InvestSummary { get; set; } = new();

    public InvestSummaryData CurrentSummaryData { get; set; }

    public IRelayCommand<object> AddNewTradeRecordCommand { get; set; }

    public InvestLogViewModel()
    {
#if DEBUG
        Debug();
#endif

        AddNewTradeRecordCommand = new RelayCommand<object>(AddNewTradeRecord);
    }

    private void AddNewTradeRecord(object observableInvestDetail)
    {
        if (observableInvestDetail is not ObservableInvestDetail investDetail) return;
        {
            TradeList.Add(investDetail);

            if (investDetail.TradeType == TradeType.Sell)
            {
                SellList.Add(investDetail);
            }
            else
            {
                BuyList.Add(investDetail);
            }
        }
    }
}