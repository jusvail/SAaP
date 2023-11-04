using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterNVol : FilterBase
{
	public FilterNVol(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

#if MONITORPAGEONLY
		if (originalDatas.Any() && originalDatas.First().CodeName == "605598")
		{
			Console.WriteLine("here");
		}
#endif
		try
		{
			var tdVol  = originalDatas[Condition.ToDays].Volume;
			var minVol = tdVol;
			for (var i = Condition.FromDays; i >= Condition.ToDays; i--)
				if (minVol > originalDatas[i].Volume)
					minVol = originalDatas[i].Volume;

			var fold = CalculationService.Round2(tdVol / minVol);

			return Compare(fold, Condition.Operator, Convert.ToDouble(Condition.RightValue));
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