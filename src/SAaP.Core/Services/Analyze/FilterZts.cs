using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterZts : FilterBase
{
	public FilterZts(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;
		if (originalDatas.Count < 2 || originalDatas.Count <= Condition.FromDays ||
		    originalDatas.Count <= Condition.ToDays) return false;

		var sum = 0;
		for (var i = 0; i < originalDatas.Count - 1; i++)
		{
			var day = originalDatas[i];
			var yes = originalDatas[i + 1];
			var zd = CalculationService.CalcTtm(yes.Ending, day.High);

			if (zd > 9.9) sum++;
		}

		return Compare(sum, Condition.Operator, Convert.ToDouble(Condition.RightValue));
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}