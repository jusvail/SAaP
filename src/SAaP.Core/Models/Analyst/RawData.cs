using System.Collections.Generic;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Models.Analyst;

public class RawData
{
    public RawData(Stock targetStock, IList<OriginalData> originalDatas)
    {
        TargetStock = targetStock;
        OriginalDatas = originalDatas;
    }

    public Stock TargetStock { get; set; }

    public IList<OriginalData> OriginalDatas { get; set; }
}