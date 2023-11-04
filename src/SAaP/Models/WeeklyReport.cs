using CommunityToolkit.Mvvm.ComponentModel;

namespace SAaP.Models;

public class WeeklyReport : ObservableRecipient
{
    private string _opDistribute;
    private string _opPercent;
    private double _averageOp;
    private string _zdDistribute;
    private string _zdPercent;
    private double _averageZf;
    private double _averageDf;
    private int _ztCount;
    private int _dtCount;
    private string _op1PPercent;

    public string OpDistribute
    {
        get => _opDistribute;
        set => SetProperty(ref _opDistribute, value);
    }

    public string OpPercent
    {
        get => _opPercent;
        set => SetProperty(ref _opPercent, value);
    }

    public double AverageOp
    {
        get => _averageOp;
        set => SetProperty(ref _averageOp, value);
    }

    public string ZdDistribute
    {
        get => _zdDistribute;
        set => SetProperty(ref _zdDistribute, value);
    }

    public string ZdPercent
    {
        get => _zdPercent;
        set => SetProperty(ref _zdPercent, value);
    }

    public double AverageZf
    {
        get => _averageZf;
        set => SetProperty(ref _averageZf, value);
    }

    public double AverageDf
    {
        get => _averageDf;
        set => SetProperty(ref _averageDf, value);
    }

    public int ZtCount
    {
        get => _ztCount;
        set => SetProperty(ref _ztCount, value);
    }

    public int DtCount
    {
        get => _dtCount;
        set => SetProperty(ref _dtCount, value);
    }

    public string Op1PPercent
    {
        get => _op1PPercent;
        set => SetProperty(ref _op1PPercent, value);
    }
}