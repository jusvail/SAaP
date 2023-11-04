using SAaP.Analyst.Models;

namespace SAaP.Analyst.Services;

public class ShortDayPatternIndicator : PatternIndicator
{

    public ShortDayPatternIndicator(ComputingData computingData, int startIndex, int endIndex) : base(computingData,
        startIndex, endIndex)
    {
    }

    public override MatchResult Indicate()
    {
        var result = new MatchResult();

        // ignore Mc>=300E stock



        return result;
    }

}