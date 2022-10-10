using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterZd : FilterBase
{
    public FilterZd(Condition condition) : base(condition)
    { }

    public override bool Filter(IList<OriginalData> originalDatas)
    {
        try
        {
            var buy = originalDatas[Condition.FromDays].Ending;
            var sell = originalDatas[Condition.ToDays].Ending;

            var zd = CalculationService.Round2(100 * (sell - buy) / buy);

            if (Compare(zd, Condition.Operator, Convert.ToDouble(Condition.RightValue)))
            {
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }
}