using CommunityToolkit.Mvvm.ComponentModel;

namespace SAaP.Models;

public partial class ReportSummary: ObservableRecipient
{
	[ObservableProperty] private string _overallAccuracyRate;
	[ObservableProperty] private string _d30AccuracyRate;
	[ObservableProperty] private string _d30AccuracyProfit;
	[ObservableProperty] private string _d30AccuracyPullback;
	[ObservableProperty] private string _d60AccuracyRate;
	[ObservableProperty] private string _d60AccuracyProfit;
	[ObservableProperty] private string _d60AccuracyPullback;

	public void Transfer(ReportSummary r)
	{
		OverallAccuracyRate = r.OverallAccuracyRate;
		D30AccuracyRate     = r.D30AccuracyRate;
		D30AccuracyProfit   = r.D30AccuracyProfit;
		D30AccuracyPullback = r.D30AccuracyPullback;
		D60AccuracyRate     = r.D60AccuracyRate;
		D60AccuracyProfit   = r.D60AccuracyProfit;
		D60AccuracyPullback = r.D60AccuracyPullback;
	}
}