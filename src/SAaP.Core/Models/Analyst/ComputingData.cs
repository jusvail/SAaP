using System;
using System.Collections.Generic;
using System.Linq;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Models.Analyst;

public class ComputingData
{
    public ComputingData(RawData rawData)
    {
        if (!rawData.OriginalDatas.Any()) throw new ArgumentOutOfRangeException();

        HistoricDataCount = rawData.OriginalDatas.Count;

        Stock = rawData.TargetStock;
        OriginalDatas = rawData.OriginalDatas;
    }

    public int HistoricDataCount { get; }

    public Stock Stock { get; }

    public IList<OriginalData> OriginalDatas { get; }

    public OriginalData this[int index] => OriginalDatas[index];


    public LineData LineData { get; set; } = new();

    public MatchResult MatchResult { get; set; } = new();
}