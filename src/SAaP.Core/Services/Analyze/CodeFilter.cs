using System;
using SAaP.Core.Models.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAaP.Core.Services.Analyze;

public class CodeFilter
{
    public string CodeName { get; set; }

    public IList<Condition> TrackCondition { get; set; }

    public IList<OriginalData> OriginalDatas { get; set; }

    private bool CheckDataAccuracy()
    {
        if (string.IsNullOrEmpty(CodeName)) return false;
        if (!TrackCondition.Any()) return false;
        if (!OriginalDatas.Any()) return false;

        return TrackCondition.Max(c => c.FromDays) < OriginalDatas.Count;
    }

    public bool Filter()
    {
        if (!CheckDataAccuracy()) return false;

        if (OriginalDatas.Count == 3)
        {

        }
        foreach (var condition in TrackCondition)
        {
            switch (condition.LeftValue)
            {
                case Condition.Op:
                    return FilterOp(condition);
            }
        }

        return false;
    }

    private bool FilterOp(Condition condition)
    {
        var bot = new AnalyzeBot(OriginalDatas);

        try
        {
            for (var i = condition.ToDays; i < condition.FromDays; i++)
            {
                if (!Compare(bot.OverpricedList[i], condition.Operator, Convert.ToDouble(condition.RightValue)))
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


    private static bool Compare(double l, string operate, double r)
    {
        return operate switch
        {
            ConditionOperator.G => l > r,
            ConditionOperator.L => l < r,
            ConditionOperator.GE => l >= r,
            ConditionOperator.LE => l <= r,
            ConditionOperator.E => Math.Abs(l - r) < 0.01,
            _ => false
        };
    }
}