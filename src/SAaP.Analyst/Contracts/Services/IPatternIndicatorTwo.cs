using SAaP.Analyst.Models;

namespace SAaP.Analyst.Contracts.Services;

public interface IPatternIndicatorTwo
{
    MatchResult IndicateTwo(ComputingData computingData, int startIndex, int endIndex);
}