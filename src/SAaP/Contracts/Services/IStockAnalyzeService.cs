using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Models;

namespace SAaP.Contracts.Services;

public interface IStockAnalyzeService
{
	Task<AnalysisResultDetail> AnalyzeAsync(string codeName, int duration);

	string CalcRelationPercent(IList<IList<double>> compare);

	IAsyncEnumerable<Stock> Filter(IEnumerable<Stock> stocks, List<ObservableTrackCondition> trackConditions, DateTimeOffset lastTradingDate,
								   CancellationToken cancellationToken);
	
	Task<BuySimulateReport> Simulate(SimulateConfiguration configuration, CancellationToken cancellationToken);

	Task<StatisticsReport>  Statistics(StatisticsConfiguration configuration, CancellationToken cancellationToken);
}