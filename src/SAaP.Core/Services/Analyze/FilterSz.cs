using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterSz : FilterBase
{
	public FilterSz(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;

		var codeName = originalDatas[0].CodeName;

		return codeName.StartsWith("00");
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}