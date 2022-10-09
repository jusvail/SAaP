using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Models;

namespace SAaP.Contracts.Services;

public interface IStockAnalyzeService
{
    Task<AnalysisResultDetail> Analyze(string codeName, int duration);

    string CalcRelationPercent(IList<IList<double>> compare);

    IAsyncEnumerable<string> Filter(IEnumerable<string> codeNames, List<ObservableTrackCondition> trackConditions, CancellationToken cancellationToken);
}