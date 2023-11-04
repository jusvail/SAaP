using System.Collections.ObjectModel;

namespace SAaP.Models;

public class BuySimulateReport
{
	public ObservableCollection<SimulateResult> SimulateResults { get; set; } = new();

	public ReportSummary ReportSummary { get; set; } = new();
}
