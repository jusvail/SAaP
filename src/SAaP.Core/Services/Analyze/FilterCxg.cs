using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;
#if MONITORPAGEONLY
using System.Linq;
#endif

namespace SAaP.Core.Services.Analyze;

public class FilterCxg : FilterBase
{
	public FilterCxg(Condition condition) : base(condition)
	{
		AsyncFilter = true;
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}

	public override async Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		//if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

#if DEBUG
		if (originalDatas.Any() &&
		    (originalDatas.First().CodeName == "600082" || originalDatas.First().CodeName == "001231"))
			Console.WriteLine("ds");
#endif
		try
		{
			if (!originalDatas.Any()) return false;

			var first = originalDatas.First();

			var count = await DbService.TakeOriginalDataCount(first.CodeName, first.BelongTo);

			var expectC = Convert.ToDouble(Condition.RightValue);

			var comp = Compare(count, Condition.Operator, expectC);
			return comp;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			return false;
		}
	}
}