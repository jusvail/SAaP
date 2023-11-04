using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAaP.Core.Services.Analyze;

public abstract class FilterBase
{
    protected Condition Condition { get; }

    protected FilterBase(Condition condition)
    {
        Condition = condition;
	}

    public bool AsyncFilter { get; set; } = false;

    public abstract bool Filter(IList<OriginalData> originalDatas);

    public abstract Task<bool> FilterAsync(IList<OriginalData> originalDatas);

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