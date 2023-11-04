using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterJumpHigh : FilterZd
{
	public FilterJumpHigh(Condition condition) : base(condition)
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
			var buy = originalDatas[Condition.FromDays].High;
			var sell = originalDatas[Condition.ToDays].Low;

			var tk = CalculationService.Round2(CalculationService.CalcTtm(buy, sell));

			return Compare(tk, Condition.Operator, Convert.ToDouble(Condition.RightValue));
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