using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;

namespace SAaP.Models;

public class HistoryDeduceData : ObservableRecipient
{
    private DateTimeOffset _preLoadDateStart = DateTimeOffset.Now.AddDays(-6);
    private DateTimeOffset _perLoadDateEnd = DateTimeOffset.Now.AddDays(-1);
    private DateTimeOffset _analyzeEndDate = DateTimeOffset.Now;

    public MonitorCondition MonitorCondition { get; set; }

    public DateTimeOffset PreLoadDateStart
    {
        get => _preLoadDateStart;
        set => SetProperty(ref _preLoadDateStart, value);
    }

    public DateTimeOffset PerLoadDateEnd
    {
        get => _perLoadDateEnd;
        set => SetProperty(ref _perLoadDateEnd, value);
    }

    public DateTimeOffset AnalyzeEndDate
    {
        get => _analyzeEndDate;
        set => SetProperty(ref _analyzeEndDate, value);
    }

    public ObservableCollection<Stock> MonitorStocks { get; set; } = new();

}