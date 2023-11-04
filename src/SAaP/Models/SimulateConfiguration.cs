using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class SimulateConfiguration
{
	public ObservableTaskDetail TaskDetail { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public List<Stock> Stocks { get; set; }

	public event EventHandler<NotifyUserEventArgs> OnAnalyzeCallback;

	public virtual void OnOnAnalyzeCallback(NotifyUserEventArgs e)
	{
		OnAnalyzeCallback?.Invoke(this, e);
	}
}