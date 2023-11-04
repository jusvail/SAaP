using SAaP.Core.Models.DB;

namespace SAaP.Analyst.Models;

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