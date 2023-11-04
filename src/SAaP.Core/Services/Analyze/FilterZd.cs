using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze;

public class FilterZd : FilterBase
{
	public FilterZd(Condition condition) : base(condition)
	{
	}

	protected double ZdCalculate(IList<OriginalData> originalDatas, int left, int right)
	{
		var buy = originalDatas[left].Ending;
		var sell = originalDatas[right].Ending;

		var zd = CalculationService.Round2(100 * (sell - buy) / buy);

		return zd;
	}

	protected double CalculateHzd(IList<OriginalData> originalDatas)
	{
		var minIndex = Condition.FromDays;
		for (var i = Condition.FromDays; i >= Condition.ToDays; i--)
		{
			if (originalDatas[i].Opening < originalDatas[minIndex].Opening)
			{
				minIndex = i;
			}
		}

		var maxIndex = Condition.FromDays;
		for (var i = Condition.FromDays; i >= Condition.ToDays; i--)
		{
			if (originalDatas[i].Ending > originalDatas[maxIndex].Ending)
			{
				maxIndex = i;
			}
		}

		// the data is reversed
		if (minIndex > maxIndex)
		{
			return ZdCalculate(originalDatas, minIndex, maxIndex);
		}

		var minIndex1 = maxIndex;
		for (var i = maxIndex; i < originalDatas.Count; i++)
		{
			if (originalDatas[i].Opening < originalDatas[minIndex1].Opening)
			{
				minIndex1 = i;
			}
		}

		var zdl = CalculationService.CalcTtm(originalDatas[minIndex1].Opening, originalDatas[maxIndex].Opening);

		var maxIndex1 = minIndex;
		for (var i = minIndex; i >= Condition.ToDays; i--)
		{
			if (originalDatas[i].Ending > originalDatas[maxIndex1].Ending)
			{
				maxIndex1 = i;
			}
		}

		var zdr = CalculationService.CalcTtm(originalDatas[minIndex].Opening, originalDatas[maxIndex1].Opening);

		return zdl > zdr ? ZdCalculate(originalDatas, minIndex1, maxIndex) : ZdCalculate(originalDatas, minIndex, maxIndex1); 
	}


	protected double CalculateMzd(IList<OriginalData> originalDatas)
	{
		var minIndex = Condition.FromDays;
		for (var i = Condition.FromDays; i >= Condition.ToDays; i--)
		{
			if (originalDatas[i].Opening < originalDatas[minIndex].Opening)
			{
				minIndex = i;
			}
		}

		return ZdCalculate(originalDatas, minIndex, Condition.ToDays);
	}

	public override bool Filter(IList<OriginalData> originalDatas)
	{
		if (originalDatas.Count <= Condition.FromDays || originalDatas.Count <= Condition.ToDays) return false;

		try
		{

			var zd = ZdCalculate(originalDatas, Condition.FromDays, Condition.ToDays);

			return Compare(zd, Condition.Operator, Convert.ToDouble(Condition.RightValue));
			// var buy = originalDatas[Condition.FromDays].Ending;
			// var sell = originalDatas[Condition.ToDays].Ending;
			// var zd = CalculationService.Round2(100 * (sell - buy) / buy);
			// return Compare(zd, Condition.Operator, Convert.ToDouble(Condition.RightValue));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
			return false;
		}
	}

	public override Task<bool> FilterAsync(IList<OriginalData> originalDatas)
	{
		throw new NotImplementedException();
	}
}