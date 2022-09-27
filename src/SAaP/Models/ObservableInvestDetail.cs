using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Models.DB;
using Mapster;
using SAaP.Constant;
using SAaP.Core.Models;

namespace SAaP.Models;

public class ObservableInvestDetail : ObservableRecipient
{
    private DateTime _tradeDate = DateTime.Now;
    private string _tradeTime = PjConstant.DefaultStartTimeSpan;
    private TradeType _tradeType = TradeType.Unknown;
    private int _volume;
    private double _price;
    private double _amount;

    public int TradeIndex { get; set; }

    public bool Editable { get; set; } = true;

    public DateTime TradeDate
    {
        get => _tradeDate;
        set => SetProperty(ref _tradeDate, value);
    }

    public string TradeTime
    {
        get => _tradeTime;
        set => SetProperty(ref _tradeTime, value);
    }

    public TradeType TradeType
    {
        get => _tradeType;
        set
        {
            if (value == TradeType.Unknown) return;
            SetProperty(ref _tradeType, value);
        }
    }

    public int Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public double Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public double Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, Volume * Price);
    }

    public InvestDetail ToInvestDetail()
    {
        return this.Adapt<InvestDetail>();
    }
}