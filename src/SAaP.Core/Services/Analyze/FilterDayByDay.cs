using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAaP.Core.Services.Analyze;

public class FilterDayByDay : FilterBase
{
    public FilterDayByDay(Condition condition) : base(condition)
    { }

    public override bool Filter(IList<OriginalData> originalDatas)
    {
        // TODO in dev
        return false;
    }

    public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
    {
	    throw new NotImplementedException();
    }
}