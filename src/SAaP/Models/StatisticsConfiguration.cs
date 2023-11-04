using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class StatisticsConfiguration
{
	public ObservableStatisticsDetail TaskDetail { get; set; }

	public List<Stock> Stocks { get; set; }

	public event EventHandler<NotifyUserEventArgs> OnAnalyzeCallback;

	public virtual void OnOnAnalyzeCallback(NotifyUserEventArgs e)
	{
		OnAnalyzeCallback?.Invoke(this, e);
	}
}