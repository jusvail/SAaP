using SAaP.Core.Models;
using SAaP.Core.Services.Analyze;

namespace SAaP.Contracts.Services;

public interface IStockAnalyzeService
{
    Task Analyze(string codeName, int duration, Action<AnalysisResultDetail> callback);

    Task GetTtm(string codeName, int duration, Action<AnalyzeBot> callback);
}