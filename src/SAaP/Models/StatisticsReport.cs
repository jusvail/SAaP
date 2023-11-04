using System.Collections.ObjectModel;

namespace SAaP.Models;

public class StatisticsReport
{
	public Dictionary<double, ObservableCollection<StatisticsResult>> StatisticsResults { get; set; } = new ();

	public ObservableStatisticsDetail TaskDetail { get; set; }
	//public ReportSummary ReportSummary { get; set; } = new();
}