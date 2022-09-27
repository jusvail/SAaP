using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Models.DB;
using SAaP.Models;

namespace SAaP.ViewModels;

public class InvestLogViewModel : ObservableRecipient
{
    public InvestLogViewModel()
    {
        BuyList.Add(new ObservableInvestDetail
        {
            Price = 56.0,
            Volume = 2000,
            TradeDate = DateTime.Now,
            TradeType = TradeType.Buy,
            TradeTime = "10:31"
        });

        BuyList.Add(new ObservableInvestDetail
        {
            Price = 51.0,
            Volume = 122000,
            TradeDate = new DateTime(2021,12,2),
            TradeType = TradeType.Buy,
            TradeTime = "14:31"
        });

        SellList.Add(new ObservableInvestDetail
        {
            Price = 12.0,
            Volume = 2400,
            TradeType = TradeType.Sell,
            TradeDate = new DateTime(2008, 12, 2),
            TradeTime = "9:31"
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

    public ObservableCollection<ObservableInvestDetail> BuyList { get; } = new();

    public ObservableCollection<ObservableInvestDetail> SellList { get; } = new();

    public ObservableCollection<InvestSummaryData> InvestSummary { get; set; } = new();

    public InvestSummaryData CurrentSummaryData { get; set; }

}