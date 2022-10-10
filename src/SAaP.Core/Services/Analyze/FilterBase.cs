using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;

namespace SAaP.Core.Services.Analyze;

public abstract class FilterBase
{
    protected Condition Condition { get; }

    protected FilterBase(Condition condition)
    {
        Condition = condition;
    }

    public abstract bool Filter(IList<OriginalData> originalDatas);

    protected static bool Compare(double l, string operate, double r)
    {
        return operate switch
        {
            ConditionOperator.G => l > r,
            ConditionOperator.L => l < r,
            ConditionOperator.Ge => l >= r,
            ConditionOperator.Le => l <= r,
            ConditionOperator.E => Math.Abs(l - r) < 0.01,
            _ => false
        };
    }
}