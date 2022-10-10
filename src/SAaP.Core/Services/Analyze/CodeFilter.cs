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

    protected bool CheckDataAccuracy()
    {
        if (string.IsNullOrEmpty(CodeName)) return false;
        if (!TrackCondition.Any()) return false;
        if (!OriginalDatas.Any()) return false;

        return TrackCondition.Max(c => c.FromDays) < OriginalDatas.Count;
    }

    public bool FilterAll()
    {
        return CheckDataAccuracy()
               &&
               TrackCondition
                   .Select(CodeFilterFactory.Create)
                   .All(filter => filter.Filter(OriginalDatas));
    }
}