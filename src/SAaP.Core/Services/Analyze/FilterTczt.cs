using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterTczt : FilterZd
{
	public FilterTczt(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

		try
		{
			var buy = originalDatas[Condition.FromDays].Ending;
			var sell = originalDatas[Condition.ToDays].High;

			var zd = CalculationService.Round2(100 * (sell - buy) / buy);

			return zd > Convert.ToDouble(Condition.RightValue);
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