using SAaP.Core.Models.Analyst;

namespace SAaP.Core.Contracts.Services;

public interface IPatternIndicator
{
    MatchResult Indicate();
}