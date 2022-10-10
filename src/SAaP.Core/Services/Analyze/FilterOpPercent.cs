using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterOpPercent : FilterBase
{
    public FilterOpPercent(Condition condition) : base(condition)
    { }

    public override bool Filter(IList<OriginalData> originalDatas)
    {
        var bot = new AnalyzeBot(originalDatas);

        var rValues = Condition.RightValue.Split(Condition.ValueSeparator);

        if (rValues.Length != 2) return false;

        try
        {
            var percent = Convert.ToDouble(rValues[0]);
            var value = Convert.ToDouble(rValues[1]) * 0.01;

            var sumDay = Condition.FromDays - Condition.ToDays;
            var targetDay = 0.0;

            for (var i = Condition.ToDays; i < Condition.FromDays; i++)
            {
                if (Compare(bot.OverpricedList[i], Condition.Operator, percent))
                {
                    targetDay++;
                }
            }

            if (targetDay >= sumDay) return true;
            if (targetDay / sumDay >= value) return true;
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }
}