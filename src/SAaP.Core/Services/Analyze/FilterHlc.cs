using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

#if MONITORPAGEONLY || DEBUG

using System.Linq;
#endif

#if MONITORPAGEONLY
using System.Linq;
#endif

namespace SAaP.Core.Services.Analyze;

public class FilterHlc : FilterZd
{
	public FilterHlc(Condition condition) : base(condition)
	{
	}

	protected int CalcHorizon(IList<OriginalData> originalDatas, int startIndex)
	{
		var hor = originalDatas[startIndex].Ending;

		var count = 0;

		for (var i = startIndex + 1; i < originalDatas.Count; i++)
		{
			var day = originalDatas[i];

			if (day.Low > hor)
			{
				count++;
				continue;
			}

			if (day.Opening > hor && day.Ending < hor)
			{
				count++;
				continue;
			}

			if (day.Opening < hor && day.Ending > hor)
			{
				count++;
				continue;
			}

			if (Math.Abs(day.Opening - hor) <= 0.01 || Math.Abs(day.Ending - hor) <= 0.01)
			{
				count++;
			}
		}

		return count;
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		//if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

#if MONITORPAGEONLY || DEBUG
		if (DateTime.Parse(originalDatas.First().Day) < DateTime.Today.AddDays(-1))
		{
			Console.WriteLine();
		}
#endif

		try
		{
			const int start = 0;
			var expectC = Convert.ToDouble(Condition.RightValue);
			var count = CalcHorizon(originalDatas, start);

			var comp = Compare(count, Condition.Operator, expectC);

			if (!comp) return false;

			return ZdCalculate(originalDatas, originalDatas.Count - 1, start) > 0;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			return false;
		}
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}