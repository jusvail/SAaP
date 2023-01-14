using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Contracts.Enum;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class ObservableMonitorDetail : ObservableRecipient
{
    private double _stopLoss;
    private Stock _stock;
    private MonitorType _monitorType;
    private List<ObservableBuyMode> _buyModes =  BuyMode.All().ToList();
    private double _stopProfitL;
    private double _stopProfitI;
    private double _stopProfitA;

    public Stock Stock
    {
        get => _stock;
        set => SetProperty(ref _stock, value);
    }

    public MonitorType MonitorType
    {
        get => _monitorType;
        set => SetProperty(ref _monitorType, value);
    }

    public List<ObservableBuyMode> BuyModes
    {
        get => _buyModes;
        set => SetProperty(ref _buyModes, value);
    }

    public double StopLoss
    {
        get => _stopLoss;
        set => SetProperty(ref _stopLoss, value);
    }

    public double StopProfitL
    {
        get => _stopProfitL;
        set => SetProperty(ref _stopProfitL, value);
    }

    public double StopProfitI
    {
        get => _stopProfitI;
        set => SetProperty(ref _stopProfitI, value);
    }

    public double StopProfitA
    {
        get => _stopProfitA;
        set => SetProperty(ref _stopProfitA, value);
    }
}