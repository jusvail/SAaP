using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;

namespace SAaP.Models;

public class HistoryDeduceData : ObservableRecipient
{
    private DateTimeOffset _preLoadDateStart = DateTimeOffset.Now.AddHours(DateTimeOffset.Now.Hour * -1).AddDays(-6);
    private DateTimeOffset _perLoadDateEnd = DateTimeOffset.Parse(DateTimeOffset.Now.ToString("yyyy/MM/dd") + " 15:00:00").AddDays(-1);
    private DateTimeOffset _analyzeEndDate = DateTimeOffset.Now.AddHours(DateTimeOffset.Now.Hour * -1).AddHours(18);

    public MonitorCondition MonitorCondition { get; set; } = new();

    public DateTimeOffset PreLoadDateStart
    {
        get => _preLoadDateStart;
        set
        {
            var newV = value.AddHours(value.Hour * -1);
            SetProperty(ref _preLoadDateStart, newV);
        }
    }

    public DateTimeOffset PerLoadDateEnd
    {
        get => _perLoadDateEnd;
        set
        {
            var newV = value.ToString("yyyy/MM/dd");
            SetProperty(ref _perLoadDateEnd, DateTimeOffset.Parse(newV + " 15:00:00"));
        }
    }

    public DateTimeOffset AnalyzeEndDate
    {
        get => _analyzeEndDate;
        set
        {
            var newV = value.AddHours(value.Hour * -1).AddHours(18);
            SetProperty(ref _analyzeEndDate, newV);
        }
    }

    public ObservableCollection<Stock> MonitorStocks { get; set; } = new();

}