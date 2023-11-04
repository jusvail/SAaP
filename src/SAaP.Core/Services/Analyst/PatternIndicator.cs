using System;
using System.Collections.Generic;
using SAaP.Core.Contracts.Services;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyst;

public enum PatternType
{
	NFoldPattern,
	FiveDay40Pattern
}

public abstract class PatternIndicator : IPatternIndicator
{
	// protected const int MinimalLasting = 20;
	// protected const int OneYear = 250;
	protected const double CciIndicate = 100d;

	protected double[] Cci;
	protected CciIndicate[] CciBs;
	protected ComputingData ComputingData;
	protected double[] D05;
	protected double[] D10;
	protected double[] D120;
	protected double[] D150;
	protected double[] D20;
	protected double[] D200;
	protected double[] D50;
	protected int EndIndex;
	protected List<OriginalData> Ori;
	protected int StartIndex;
	protected List<int> Volume;

	public abstract MatchResult Indicate();

	public static PatternIndicator Create(PatternType type)
	{
		return type switch
		{
			PatternType.NFoldPattern => new NFoldPatternIndicator(),
			PatternType.FiveDay40Pattern => new FiveDay40PatternIndicator(),
			_ => throw new ArgumentException("not enough information")
		};
	}

	public PatternIndicator InitField(ComputingData computingData, int startIndex, int endIndex)
	{
		StartIndex = startIndex;
		EndIndex = endIndex;
		ComputingData = computingData;

		var range = endIndex - startIndex + 1;

		D05 = new double[range];
		D10 = new double[range];
		D20 = new double[range];
		D50 = new double[range];
		D120 = new double[range];
		D150 = new double[range];
		D200 = new double[range];
		Volume = new List<int>();
		Ori = new List<OriginalData>();

		Cci = new double[range];
		CciBs = new CciIndicate[range];

		ListReInitialize(computingData, startIndex, endIndex);

		return this;
	}

	private void ListReInitialize(ComputingData computingData, int startIndex, int endIndex)
	{
		var nlIndex = 0;

		#region INITIALIZE NEEDED DATA

		try
		{
			for (var i = startIndex; i <= endIndex; i++, nlIndex++)
			{
				var tdd = computingData.OriginalDatas[i];
				Ori.Add(tdd);

				D05[nlIndex] = computingData.LineData[LineForm.D5][i];
				D10[nlIndex] = computingData.LineData[LineForm.D10][i];

				D20[nlIndex] = computingData.LineData[LineForm.D20][i];
				D50[nlIndex] = computingData.LineData[LineForm.D50][i];
				D120[nlIndex] = computingData.LineData[LineForm.D120][i];
				D150[nlIndex] = computingData.LineData[LineForm.D150][i];
				D200[nlIndex] = computingData.LineData[LineForm.D200][i];
				Volume.Add(computingData[i].Volume);

				Cci[nlIndex] = computingData.LineData[LineForm.Cci][i];

				CciBs[nlIndex] = computingData.LineData[LineForm.Cci][i - 1] switch
				{
					< -CciIndicate when computingData.LineData[LineForm.Cci][i] > -CciIndicate => Models.Analyst
						.CciIndicate
						.BuyTriggered,
					> CciIndicate when computingData.LineData[LineForm.Cci][i] < CciIndicate => Models.Analyst
						.CciIndicate
						.SellTriggered,
					_ => computingData.LineData[LineForm.Cci][i] switch
					{
						> 200 => Models.Analyst.CciIndicate.ExtremelyHigh,
						> 100 => Models.Analyst.CciIndicate.High,
						< -200 => Models.Analyst.CciIndicate.ExtremelyLow,
						< -100 => Models.Analyst.CciIndicate.Low,
						_ => Models.Analyst.CciIndicate.Normal
					}
				};
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(GetType());
		}

		#endregion INITIALIZE NEEDED DATA
	}

	protected static bool IndicateStep3Start(IList<OriginalData> data, ref int j)
	{
		var pattern1 = CalculationService.CalcTtm(data[j - 2].Opening, data[j].High) > 15d // 3d pl > 15%
					   && CalculationService.CalcTtm(data[j - 2].Opening, data[j - 2].High) > 7d; // j-3 max pull-up>7%
		if (pattern1) j -= 2;

		var pattern2 = CalculationService.CalcTtm(data[j - 1].Opening, data[j - 1].High) > 9.8d // yesterday z-hang ting
					   && CalculationService.CalcTtm(data[j - 1].Ending, data[j].High) > 1d; // td max pull-up>3%
		if (pattern2) j -= 1;

		return pattern1 || pattern2;
	}

	protected static void AccuracyStep1(IList<OriginalData> data, ref int start, ref int range)
	{
		if (start == 0 || range == 0) return;

		var i = start;
		var e = start + range;
		// find lowest price right to start
		while (!(CalculationService.CalcTtm(data[i - 1].Ending, data[i].Ending) > 5d &&
					CalculationService.CalcTtm(data[i - 1].Ending, data[i + 3].Ending) > 15d))
		{
			i++;
			if (i >= e) break;
		}

		var offset = i - start;
		// while (data[i + 1].Opening < data[start].Opening) i++;
		if (offset > 0 && i != e) start = i;

		var re = e >= data.Count ? data.Count - 1 : e;

		for (var j = start; j <= Math.Min(e + offset, data.Count - 1); j++)
			if (data[j].Ending > data[re].Ending)
				re = j;
		// find highest price right to original start+range
		//while (data[e + 1].Ending > data[e].Ending) e++;

		range = re - i + 1;
	}

	protected static bool IndicateStep1(IList<OriginalData> data, int start, int range)
	{
		var hIndex = start;

		for (var j = hIndex + 1; j <= start + range; j++)
			if (data[j].High > data[hIndex].High)
				hIndex = j;

		return Zh(data, start, hIndex) > 95;
	}

	protected static double Zh(IList<OriginalData> data, int startIndex, int endIndex)
	{
		return CalculationService.CalcTtm(data[startIndex].Opening, data[endIndex].High);
	}

	protected static double Zd(IList<OriginalData> data, int startIndex, int endIndex)
	{
		try
		{
			return CalculationService.CalcTtm(data[startIndex].Opening, data[endIndex].Ending);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(nameof(PatternIndicator) + nameof(Zd));
			throw;
		}
	}

	#region USELESS CURRENTLY

	// protected bool RangeCheckAll(ComputingData computingData, int start, int end, Func<ComputingData, int, bool> func)
	// {
	//	 if (start < 0) return false;
	//
	//	 for (var i = start; i <= end; i++)
	//		 if (!func(computingData, i))
	//			 return false;
	//
	//	 return true;
	// }
	//
	// protected void RangeCalcAll<T>(ComputingData computingData, int start, int end, Action<ComputingData, int> action)
	// {
	//	 if (start < 0) return;
	//
	//	 for (var i = start; i <= end; i++)
	//		 action(computingData, i);
	//
	// }
	//
	// protected IEnumerable<T> RangeGetAll<T>(ComputingData computingData, int start, int end,
	//	 Func<ComputingData, int, T> func)
	// {
	//	 if (start < 0) yield break;
	//
	//	 for (var i = start; i <= end; i++)
	//		 yield return func(computingData, i);
	// }

	#endregion USELESS CURRENTLY
}