using SAaP.Core.Models;

namespace SAaP.Contracts.Services;

public interface IStockAnalyzeService
{
    Task Analyze(string codeName, int duration, Action<AnalysisResult> callback);
}