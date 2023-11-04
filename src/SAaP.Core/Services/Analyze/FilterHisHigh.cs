﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterHisHigh : FilterZd
{
	public FilterHisHigh(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

#if MONITORPAGEONLY

		if (originalDatas.Any() && originalDatas.First().CodeName == "603069")
		{
			Console.WriteLine("here");
		}
#endif
		try
		{
			var hzd = CalculateHzd(originalDatas);
			var mzd = CalculateMzd(originalDatas);

			var zd = Math.Abs(CalculationService.Round2(100 * (hzd - mzd) / hzd));

			if (zd > 10)
			{
				return false;
			}

			return Compare(zd, Condition.Operator, Convert.ToDouble(Condition.RightValue));
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