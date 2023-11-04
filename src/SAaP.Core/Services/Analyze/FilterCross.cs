using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterCross : FilterBase
{
	public FilterCross(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

		var allP = new List<double>();
		for (var i = Condition.FromDays; i >= Condition.ToDays; i--)
		{
			allP.Add(originalDatas[i].Low);
			allP.Add(originalDatas[i].High);
		}

		var minPrice = allP.Min();
		var maxPrice = allP.Max();

		var crossed = false;
		var incr = minPrice;
		while (incr <= maxPrice)
		{
			var i = Condition.ToDays;
			for (; i <= Condition.FromDays; i++)
			{
				if (originalDatas[i].Low < incr && originalDatas[i].High > incr)
					continue;
				break;
			}

			if (i == 5)
			{
				crossed = true;
				break;
			}
			incr += 0.01;
		}

		return crossed;
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new System.NotImplementedException();
	}
}