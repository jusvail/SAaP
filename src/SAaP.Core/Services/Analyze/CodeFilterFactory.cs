namespace SAaP.Core.Services.Analyze;

public class CodeFilterFactory
{
    private CodeFilterFactory()
    { }

    public static FilterBase Create(Condition condition)
    {
        return condition.LeftValue switch
        {
            Condition.Op => new FilterOp(condition),
            Condition.Zd => new FilterZd(condition),
            Condition.OpPercent => new FilterOpPercent(condition),
            Condition.To => new FilterDayByDay(condition),
            Condition.Th => new FilterDayByDay(condition),
            Condition.Tl => new FilterDayByDay(condition),
            Condition.Te => new FilterDayByDay(condition),
            Condition.Yo => new FilterDayByDay(condition),
            Condition.Yh => new FilterDayByDay(condition),
            Condition.Yl => new FilterDayByDay(condition),
            Condition.Ye => new FilterDayByDay(condition),
            _ => new FilterOp(condition)
        };
    }
}