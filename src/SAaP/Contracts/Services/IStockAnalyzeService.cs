using SAaP.Core.Models;
using SAaP.Core.Models.DB;

namespace SAaP.Contracts.Services;

public interface IStockAnalyzeService
{
    Task<AnalysisResultDetail> Analyze(string codeName, int duration);

    string CalcRelationPercent(IList<IList<double>> compare);
}