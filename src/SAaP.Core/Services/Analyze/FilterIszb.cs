using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterIszb : FilterBase
{
	public FilterIszb(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;

		var codeName = originalDatas[0].CodeName;

		return codeName.StartsWith("00") || codeName.StartsWith("60");
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new System.NotImplementedException();
	}
}