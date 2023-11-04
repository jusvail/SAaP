using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterSh : FilterBase
{
	public FilterSh(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;
#if DEBUG
		if (originalDatas.First().CodeName == "110801")
		{
			Console.WriteLine();
		}
#endif
		var codeName = originalDatas[0].CodeName;

		return codeName.StartsWith("60");
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}