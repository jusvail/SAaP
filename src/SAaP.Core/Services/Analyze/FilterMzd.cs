using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterMzd : FilterZd
{
	public FilterMzd(Condition condition) : base(condition)
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
			var mzd = CalculateMzd(originalDatas);

			return Compare(mzd, Condition.Operator, Convert.ToDouble(Condition.RightValue));
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