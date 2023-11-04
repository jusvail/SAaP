using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze;

public class FilterOpPercent : FilterBase
{
	public FilterOpPercent(Condition condition) : base(condition)
	{
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (!originalDatas.Any() || originalDatas.Count < 2) return false;

		var bot = new AnalyzeBot(originalDatas);

		var rValues = Condition.RightValue.Split(Condition.ValueSeparator);

		if (rValues.Length != 2) return false;

		try
		{
			var percent = Convert.ToDouble(rValues[0]);
			var value = Convert.ToDouble(rValues[1]) * 0.01;

			var sumDay = Condition.FromDays - Condition.ToDays;
			var targetDay = 0.0;

			for (var i = Condition.ToDays; i < Condition.FromDays; i++)
				if (bot.ActualCount - i - 1 >= 0 && Compare(bot.OverpricedList[bot.ActualCount - i - 1], Condition.Operator, percent))
					targetDay++;

			if (targetDay >= sumDay) return true;
			if (targetDay / sumDay >= value) return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			return false;
		}

		return false;
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}