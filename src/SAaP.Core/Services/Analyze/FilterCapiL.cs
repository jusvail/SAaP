using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Helpers;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Api;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterCapiL : FilterBase
{
	public FilterCapiL(Condition condition) : base(condition)
	{
		AsyncFilter = true;
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}

	public override async Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

		var amountAll = 0d;
		for (var i = Condition.ToDays; i <= Condition.FromDays; i++)
		{
			var avP = (originalDatas[i].Opening + originalDatas[i].Ending) / 2;
			amountAll += avP * originalDatas[i].Volume / 100000000;
		}

		var first = originalDatas.First();

		var webString =
			await Http.GetStringAsync(
				WebServiceApi.GenerateTxQueryString(
					StockService.ReplaceFlagToLocString(first.CodeName, first.BelongTo)));
		
		var executed = webString.Split("~");

		if (executed.Length < 6) return false;

		var capiNow = Convert.ToDouble(executed[44]); // 流通市值
		
		var lastTradingDay = await DbService.TakeOriginalDataFromFile(first.CodeName, first.BelongTo, 1);
		var last           = lastTradingDay.First();

		if (originalDatas[Condition.ToDays].Day != last.Day)
		{
			// 需要换算下当日市值
			capiNow = capiNow * originalDatas[Condition.ToDays].Ending / last.Ending;
		}

		var fold = amountAll / capiNow;

		return Compare(fold, Condition.Operator, Convert.ToDouble(Condition.RightValue));
	}
}