using System;
using SAaP.Core.Models.DB;
using System.Collections.Generic;

namespace SAaP.Core.Services.Analyze;

public class CodeFilter
{
    public string CodeName { get; set; }

    public IList<Condition> TrackCondition { get; set; }

    public IList<OriginalData> OriginalDatas { get; set; }

    public bool Filter()
    {
        var random = new Random();

        var f = random.Next(0, 1000);
        return f < 10;
    }
}