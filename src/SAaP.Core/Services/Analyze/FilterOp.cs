using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterOp : FilterBase
{
    public FilterOp(Condition condition) : base(condition)
    { }

    public override bool Filter(IList<OriginalData> originalDatas)
    {
        var bot = new AnalyzeBot(originalDatas);

        try
        {
            for (var i = Condition.ToDays; i < Condition.FromDays; i++)
            {
                if (!Compare(bot.OverpricedList[i], Condition.Operator, Convert.ToDouble(Condition.RightValue)))
                {
                    return false;
                }
            }

        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}