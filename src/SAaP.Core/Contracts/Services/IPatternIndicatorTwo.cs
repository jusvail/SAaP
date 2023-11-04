using SAaP.Core.Models.Analyst;

namespace SAaP.Core.Contracts.Services;

public interface IPatternIndicatorTwo
{
    MatchResult IndicateTwo(ComputingData computingData, int startIndex, int endIndex);
}