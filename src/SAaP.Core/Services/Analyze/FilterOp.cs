using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterOp : FilterBase
{
	public FilterOp(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any()) return false;

		var bot = new AnalyzeBot(originalDatas);

		try
		{
			for (var i = Condition.ToDays; i < Condition.FromDays; i++)
				if (!Compare(bot.OverpricedList[bot.ActualCount - i - 1], Condition.Operator, Convert.ToDouble(Condition.RightValue)))
					return false;
		}
		catch (Exception)
		{
			return false;
		}

		return true;
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}