using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Services.Generic;

namespace SAaP.Models;

public class ObservableInvestSummaryDetail : ObservableRecipient
{
    private int _tradeIndex;
    private string _codeName;
    private string _companyName;
    private DateTime _start;
    private DateTime _end;
    private double _averageCost;
    private double _averageSell;
    private double _profit;
    private bool _isArchived;
    private bool _isArchivedAndSavedToDb;
    private int _volume;

    public int TradeIndex
    {
        get => _tradeIndex;
        set => SetProperty(ref _tradeIndex, value);
    }

    public string CodeName
    {
        get => _codeName;
        set => SetProperty(ref _codeName, value);
    }

    public string CompanyName
    {
        get => _companyName;
        set => SetProperty(ref _companyName, value);
    }

    public DateTime Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }


    public DateTime End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }

    public double AverageCost
    {
        get => _averageCost;
        set => SetProperty(ref _averageCost, value);
    }

    public double AverageSell
    {
        get => _averageSell;
        set => SetProperty(ref _averageSell, value);
    }

    public double Profit
    {
        get => _profit;
        set => SetProperty(ref _profit, value);
    }

    public int Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public bool IsArchived
    {
        get => _isArchived;
        set => SetProperty(ref _isArchived, value);
    }

    public bool IsArchivedAndSavedToDb
    {
        get => _isArchivedAndSavedToDb;
        set => SetProperty(ref _isArchivedAndSavedToDb, value);
    }

    public string Earning => (CalculationService.Round2(Volume * AverageCost * Profit / 100)).ToString(CultureInfo.InvariantCulture);
    public string FullTradeDateRange => $"{Start:yyyy/MM/dd} ~ {End:yyyy/MM/dd}";
    public string Status => IsArchived ? "已清仓" : "交易中";

    public void Clear()
    {
        TradeIndex = -1;
        CodeName = string.Empty;
        CompanyName = string.Empty;
        Start = DateTime.Now.Date;
        End = DateTime.Now.Date;
        AverageCost = 0.0;
        AverageSell = 0.0;
        Profit = 0.0;
        Volume = 0;
        IsArchived = false;
        IsArchivedAndSavedToDb = false;
    }

    public void EnsureArchived()
    {
        IsArchivedAndSavedToDb = IsArchived;
    }

    public void DefaultArchived()
    {
        IsArchivedAndSavedToDb = true;
    }
}