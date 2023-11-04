using SAaP.Analyst.Models;

namespace SAaP.Analyst.Contracts.Services;

public interface IPatternIndicator
{
    MatchResult Indicate();
}