using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Analyst;
using SAaP.Core.Services.Analyst.Pipe;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterPatternFoldN : FilterBase
{
	public FilterPatternFoldN(Condition condition) : base(condition)
	{
		AsyncFilter = true;
	}

	public override async Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;

#if DEBUG || MONITORPAGEONLY
		if (originalDatas.First().CodeName == "600666")
		{
			Console.WriteLine();
		}
#endif
		var codeMain = originalDatas.First().CodeName;
		var belong = originalDatas.First().BelongTo;

		// query original data recently
		var data = await DbService.TakeOriginalDataAscending(codeMain, belong, int.MaxValue);

		if (!data.Any()) return false;

		var rawData = new RawData(new Stock { CodeName = codeMain, BelongTo = belong }, data);

		var plumber = new Plumber(PatternType.NFoldPattern);

		var computeData = new ComputingData(rawData);

		plumber.PreProcess(computeData);
		plumber.IndicatePattern(computeData);

		if (!computeData.MatchResult.Bought) return false;

		var td = DateTime.Parse(computeData.OriginalDatas.Last().Day);
		var found = DateTime.Parse(computeData.MatchResult.BoughtDay);

		return td.Year == found.Year && td.Month == found.Month && td.Day == found.Day;
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}