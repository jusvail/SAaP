using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class CodeFilter
{
	public string CodeName { get; init; }

	public int BelongTo { get; set; }

	public DateTime LastTradingDay { get; set; } = DateTime.Now;

	public IList<Condition> TrackCondition { get; init; }

	//public IList<OriginalData> OriginalDatas { get; init; }

	public async Task<bool> FilterAll()
	{
		if (string.IsNullOrEmpty(CodeName)) return false;
		if (!TrackCondition.Any()) return false;

		// if (!OriginalDatas.Any()) return false;
		// return TrackCondition.Max(c => c.FromDays) < OriginalDatas.Count
		// return TrackCondition.Select(CodeFilterFactory.Create).All(filter => filter.Filter(OriginalDatas));

#if DEBUG
		if (CodeName == "600636")
		{
			Console.WriteLine();
		}
#endif
		foreach (var condition in TrackCondition)
		{
			try
			{
				var ori = new List<OriginalData>();
				if (condition.IsSpecialMatch)
				{
					ori.Add(new OriginalData { CodeName = CodeName, BelongTo = BelongTo });
				}
				else
				{
					ori = await DbService.TakeOriginalData(CodeName, BelongTo, condition.FromDays, LastTradingDay);
				}
				var remove = condition.ToDays;
				if (!ori.Any() || ori.Count < remove) return false;
				for (; remove > 0; remove--)
				{
					ori.RemoveAt(0);
					condition.FromDays--;
				}

				var filter = CodeFilterFactory.Create(condition);
				if (filter.AsyncFilter)
				{
					if (!(await filter.FilterAsync(ori)))
					{
						return false;
					}
				}
				else
				{
					if (!Task.Run(() => filter.Filter(ori)).Result)
					{
						return false;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);Console.WriteLine(GetType());
				Console.WriteLine(GetType());
				return false;
			}
		}

		return true;
	}
}