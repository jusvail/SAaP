using System;
using System.Collections.Generic;
using System.Linq;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Services.Generic;

// ReSharper disable PossibleLossOfFraction

namespace SAaP.Core.Services.Analyst;

public class NFoldPatternIndicator : PatternIndicator
{
	public override MatchResult Indicate()
	{
		#region Initialize object

		var range = EndIndex - StartIndex + 1;

		// FieldReInitialize(computingData, startIndex, endIndex);

		#endregion

		#region Initialize object for step2

		// var sliderStart = 0;
		var sliderRange = 30;

		bool step1 = false, step2 = false, step3 = false, step4 = false;
		var step1StartIndex = -1;
		var step2StartIndex = -1;
		var step3StartIndex = -1;
		var step4StartIndex = -1;
		var buyIndex = -1;
		var sellIndex = -1;

		var step2Progress = 0d;

		#endregion

		var step1Times = 0;

		for (var j = 0; j < range;)
		{
			try
			{
				if (j >= range) break;

				if (!step1)
					while (true)
					{
						if (j < range && (D05[j] < D200[j] || D05[j] < D50[j]))
						{
							j++;
							continue;
						}

						break;
					}

			}
			catch (Exception e)
			{
				Console.WriteLine(e); Console.WriteLine(GetType());
				return new MatchResult
				{
					Error = true
				};
			}

			#region Step1

			try
			{

				var flashback = Math.Min(j - 0, sliderRange);
				var uselessRange = j - flashback;
				AccuracyStep1(Ori, ref flashback, ref uselessRange);

				if (step1Times < range && Math.Abs(j - flashback) < 50)
				{
					j = flashback;
					step1Times++;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e); Console.WriteLine(GetType());
				return new MatchResult
				{
					Error = true
				};
			}

			try
			{
				if (j + sliderRange >= range) break;
				if (j >= range) break;

				// start to indicate step1
				if (!step1) step1 = true;

				// indicate at lease 90 pull up within {sliderRange} day
				while (!IndicateStep1(Ori, j, sliderRange))
				{
					j++;
					// break at end of the data
					if (j + sliderRange >= range) break;
					if (j >= range) break;
				}

				// break at end of the data
				if (j + sliderRange >= range || j >= range)
				{
					step1 = false;
					break;
				}

				// make the [j, range] for step1 more accuracy
				AccuracyStep1(Ori, ref j, ref sliderRange);

				step1StartIndex = j;
				step2StartIndex = j + sliderRange;
				j = step2StartIndex;

				// indicate step2
				if (!step2) step2 = true;

				#endregion

				// ---------------------------------
				// indicate a less than 30% pullback
				// ---------------------------------

				if (j >= range) break;

				// make sure 150d line > 200d line
				while (D150[j] < D200[j])
				{
					j++;
					if (j >= range)
						break;
				}

				if (j >= range) break;

				// make sure the pullback within 30%
				if (Zd(Ori, step2StartIndex, j) < -30)
				{
					j += sliderRange;
					step1 = step2 = false;
					continue;
				}

				var maxIdx = step2StartIndex;
				var right = Math.Min(range - 1, step2StartIndex + sliderRange);
				for (var k = step2StartIndex; k <= right; k++)
				{
					if (!(CalculationService.CalcTtm(Ori[step2StartIndex].Ending, Ori[k].Ending) > 12d)) continue;
					if (Ori[maxIdx].Ending < Ori[k].Ending) maxIdx = k;
				}

				j = step2StartIndex = maxIdx;
				sliderRange = step2StartIndex - step1StartIndex;

				#region Step2

				// var cciTriggeredAtStep2 = false;
				var firstEndingLowerThan50d = false;
				var firstLowLowerThan50d    = false;
				var crossedCount            = 0;
				while (j < range)
				{
					// is not the sharp we need for now
					if (CalculationService.CalcTtm(Ori[step2StartIndex].Ending, Ori[j].Ending) > 25d)
						break;

					if (sliderRange == j - step2StartIndex) sliderRange += 30;

					// 5d line first lower than 50d line
					if (!firstLowLowerThan50d && Ori[j].Low > D50[j])
					{
						j++;
						continue;
					}
					firstLowLowerThan50d = true;

					var sameWith = new List<double>();
					var cross = new List<double>();
					for (var i = 1; i <= 7; i++)
					{
						cross.Add(Ori[j - i].High);
						cross.Add(Ori[j - i].Low);
						sameWith.Add(Math.Abs(CalculationService.CalcTtm(Ori[j - i].Ending, Ori[j].Ending)));
					}

					var d5H = cross.Max();
					var d5L = cross.Min();

					var crossed = false;
					var incr = d5L;
					while (incr <= d5H)
					{
						var i = 0;
						for (; i < 5; i++)
						{
							if (Ori[j - i].Low < incr && Ori[j - i].High > incr)
								continue;
							break;
						}

						if (i == 5)
						{
							crossed = true;
							crossedCount++;
							break;
						}
						incr += 0.01;
					}

					if (!crossed)
					{
						// 5d line first lower than 50d line
						if (!firstEndingLowerThan50d && Ori[j].Ending > D50[j])
						{
							j++;
							continue;
						}
						firstEndingLowerThan50d = true;
					}

					bool Perfect(double l, double r, double v = 2d)
					{
						return Math.Abs(CalculationService.CalcTtm(l, r)) < v;
					}

					// make sure the pullback within 30%
					var pullback = Zd(Ori, step2StartIndex, j);
					if (pullback < -30) break;

					// set a default progress using percent of (5d line<50d line)/slide range
					if (step2Progress < .1d) step2Progress = 100 * (j - step2StartIndex) / sliderRange;
					//if (step2Progress < .1d) step2Progress = ((30 - Math.Abs(pullback)) / 30) * 100 * (j - step2StartIndex) / sliderRange;

					var maxVolume = Volume.GetRange(step1StartIndex, step2StartIndex - step1StartIndex + 1).Max();
					var volTimes = maxVolume / Ori[j].Volume;
					var pb = CalculationService.CalcTtm(Ori[step2StartIndex].Ending, Ori[j].Low);

					// extremely low volume compare to super high volume in step 1
					var superiorVolume = volTimes > 9d;
					// cci buy triggered 
					var cciReadyToBuy = CciBs[j] == Models.Analyst.CciIndicate.BuyTriggered;
					// 5d,10d,20d,50d line intersect 
					var perfectLineCoincide = Perfect(D05[j], D50[j]) && Perfect(D05[j], D20[j]);
					// low amplitude < 4%
					var lowZf = CalculationService.CalcTtm(Ori[j].Low, Ori[j].High) < 5d;
					// small pull-back less than 105
					var lowPb = Ori[j].Low < Ori[step2StartIndex].Ending && pb > -10d;

					//step2Progress
					if (superiorVolume) step2Progress += volTimes;
					if (cciReadyToBuy) step2Progress += 5;
					// cciTriggeredAtStep2 = true;
					if (perfectLineCoincide) step2Progress += 5;
					if (lowZf) step2Progress += 5;
					if (lowPb) step2Progress += 1;

					if (cciReadyToBuy && perfectLineCoincide)
					{
						if (lowPb) step2Progress += 5;
						if (lowZf) step2Progress += 5;
						step2Progress += 5;
					}

					if (step2Progress > 90)
						if (pullback is < -10 and > -20)
						{
							step2Progress += 20 + pullback;
						}

					if (crossed)
					{
						step2Progress = 120;
					}

					step2Progress = sameWith.Where(smallPb => smallPb < 2).Aggregate(step2Progress, (current, smallPb) => current + 5);

					// 5d,10d,20d,50d line intersect
					var badLineCoincide = CalculationService.CalcTtm(D10[j], D05[j]);
					var badLineCoincide20 = CalculationService.CalcTtm(D20[j], D10[j]);
					var badLineCoincide50 = CalculationService.CalcTtm(D50[j], D20[j]);
					if (badLineCoincide < -2d && badLineCoincide20 < -2d && badLineCoincide50 < -1d)
					{
						step2Progress += badLineCoincide;
						step2Progress += badLineCoincide20;
						step2Progress += badLineCoincide50;
					}

					if (D50[j] < D50[j - 1]) step2Progress *= .9;

					if (Math.Abs(CalculationService.CalcTtm(D150[j], D05[j])) < 10d) step2Progress *= .9;

					if (D05[j] < D05[j - 1] && D20[j] < D20[j - 1]
											&& D05[j] < D20[j] && D05[j] < D50[j]
											&& CalculationService.CalcTtm(D05[j], D20[j]) > 5.5d
					   )
						step2Progress *= .85;

					if (step2Progress > 98)
					{
						step2 = true;
						buyIndex = j;
						break;
					}

					j++;

					if (sliderRange < j - step2StartIndex) sliderRange += 30;
				}

				#endregion

				// 603366 step2过短！已经提前拉升=> 如果step2 内有低于50日线的情况，则减去progress
				// 000721 过早卖出
				// 601975 提早卖出， step3 判断过早/step4判断过早
				// 002824 买入时，150和200日线相聚过窄
				// 605128 150和200日线相聚过窄，150/200日线在上涨
				// 600734 未卖出
				// 688697 未卖出002
				// 50日线应该持续向上！！！！！！！！

				// 筹码要很集中

				if (step2Progress < 98) break;

				// indicate step3 start
				// current price > 50d line && quick pull-up signal => step3 start
				while (!(Ori[j].Ending > D50[j]
							 && Ori[j].Ending > Ori[step2StartIndex].Ending * .85
							 && IndicateStep3Start(Ori, ref j)))
					if (++j >= range)
						break;

				if (j >= range) break;

				// indicate step3
				if (!step3)
				{
					step3 = true;
					step3StartIndex = j;
				}

				// indicate step3 over
				for (var k = step3StartIndex; k < range; k++)
				{
					if (Zd(Ori, buyIndex, k) < -10d)
					{
						step4StartIndex = sellIndex = k;
						// indicate step4
						if (!step4) step4 = true;
						break;
					}

					var yzf = CalculationService.CalcTtm(Ori[k - 1].Low, Ori[k - 1].High);
					var yzd = CalculationService.CalcTtm(Ori[k - 2].Ending, Ori[k - 1].Ending);
					var tdZf = CalculationService.CalcTtm(Ori[k].Low, Ori[k].High);
					var tdZd = CalculationService.CalcTtm(Ori[k - 1].Ending, Ori[k].Ending);

					if (yzd > 8d && tdZf > 9.9 && tdZf > yzd && tdZf > yzf && tdZd < -6)
					{
						step4StartIndex = sellIndex = k;
						// indicate step4
						if (!step4) step4 = true;
						break;
					}


					if (Zd(Ori, buyIndex, k) < 10d) continue;

					if (Math.Abs(CalculationService.CalcTtm(D05[k], D10[k])) < 5d && Ori[k].High < D10[k])
					{
						step4StartIndex = sellIndex = k;
						// indicate step4
						if (!step4) step4 = true;
						break;
					}
				}

				break;
			}
			catch (Exception e)
			{
				Console.WriteLine(e); Console.WriteLine(GetType());
				return new MatchResult
				{
					Error = true
				};
			}
		}

		#region Setting Result

		var result = new MatchResult
		{
			Step1Found = step1,
			Step2Found = step2,
			Step3Found = step3,
			Step4Found = step4
		};

		if (result.Step1Found && step1StartIndex > 0)
			// result.KeyData.Add(ori[step1StartIndex]);
			result.Message.Add(Ori[step1StartIndex < range ? step1StartIndex : range - 1].Day);

		if (result.Step2Found && step2StartIndex > 0)
			// result.KeyData.Add(ori[step2StartIndex]);
			result.Message.Add($"{Ori[step2StartIndex < range ? step2StartIndex : range - 1].Day}");

		result.BuyProgress = step2Progress;

		if (buyIndex > 0)
		{
			result.Bought = true;
			result.BoughtDay = Ori[buyIndex].Day;
			result.BuyPrice = Ori[buyIndex].Ending;
		}

		if (result.Step3Found && step3StartIndex > 0)
			// result.KeyData.Add(ori[step3StartIndex]);
			result.Message.Add($"{Ori[step3StartIndex < range ? step3StartIndex : range - 1].Day}");

		if (sellIndex > 0)
		{
			result.Sold = true;
			result.SoldDay = Ori[sellIndex].Day;
			result.SellPrice = Ori[sellIndex].Ending;
			result.HoldingDays = sellIndex - buyIndex + 1;
			result.Profit = CalculationService.CalcTtm(Ori[buyIndex].Ending, Ori[sellIndex].Ending) + "%";
		}

		if (result.Step4Found && step4StartIndex > 0)
			// result.KeyData.Add(ori[step4StartIndex]);
			result.Message.Add($"{Ori[step4StartIndex < range ? step4StartIndex : range - 1].Day}");

		#endregion

		return result;
	}

	#region ORIGINAL INDICATER

	// public override MatchResult Indicate(ComputingData computingData, int startIndex, int endIndex)
	// {
	//     var result = new MatchResult();
	//
	//     bool case1, case2, case3, case4, case5, case6, case7;
	//
	//     var lined5 = computingData.LineData[LineForm.D5];
	//     var lined20 = computingData.LineData[LineForm.D20];
	//     var lined50 = computingData.LineData[LineForm.D50];
	//     var lined120 = computingData.LineData[LineForm.D120];
	//     var lined150 = computingData.LineData[LineForm.D150];
	//     var lined200 = computingData.LineData[LineForm.D200];
	//
	//     var ccia = computingData.LineData[LineForm.Cci];
	//
	//     var range = endIndex - startIndex + 1;
	//     var case1S = new bool[range];
	//     var case2S = new bool[range];
	//     var case3S = new bool[range];
	//     var case4S = new bool[range];
	//     var case5S = new bool[range];
	//     var case6S = new bool[range];
	//     var case7S = new bool[range];
	//     var case6Sv = new double[range];
	//     var case7Sv = new double[range];
	//     var cci = new double[range];
	//     var cciBs = new CciIndicate[range];
	//
	//     var volumes = new int[range];
	//
	//     var opening = new double[range];
	//     var endings = new double[range];
	//     var high = new double[range];
	//     var low = new double[range];
	//     var zf = new double[range];
	//     var zd = new double[range];
	//
	//     var nlindex = 0;
	//
	//
	//     var d05 = new double[range];
	//     var d20 = new double[range];
	//     var d50 = new double[range];
	//     var d120 = new double[range];
	//     var d150 = new double[range];
	//     var d200 = new double[range];
	//
	//     var ori = new List<OriginalData>();
	//
	//     for (var i = startIndex; i <= endIndex; i++)
	//     {
	//         var tdd = computingData.OriginalDatas[i];
	//         ori.Add(tdd);
	//
	//         d05[nlindex] = computingData.LineData[LineForm.D5][i];
	//         d20[nlindex] = computingData.LineData[LineForm.D20][i];
	//         d50[nlindex] = computingData.LineData[LineForm.D50][i];
	//         d120[nlindex] = computingData.LineData[LineForm.D120][i];
	//         d150[nlindex] = computingData.LineData[LineForm.D150][i];
	//         d200[nlindex] = computingData.LineData[LineForm.D200][i];
	//
	//         case1S[nlindex] = tdd.Ending > lined150[i] && tdd.Ending > lined200[i];
	//         case2S[nlindex] = lined150[i] > lined200[i];
	//         // 200d line inc for 20d
	//         case3S[nlindex] = RangeCheckAll(computingData, i - MinimalLasting + 1, i,
	//             (data, index) => data.LineData[LineForm.D200][index] > data.LineData[LineForm.D200][index - 1]);
	//
	//         case4S[nlindex] = lined50[i] > lined150[i] && lined50[i] > lined200[i];
	//         case5S[nlindex] = tdd.Ending > lined50[i];
	//
	//         // get last 250d ending
	//         var lastYear = RangeGetAll(computingData, i - OneYear + 1, i, (data, index) => data[index].Ending).ToList();
	//         // inc since low/dec since high
	//         case6Sv[nlindex] = CalculationService.CalcTtm(lastYear.Min(d => d > 0 ? d : double.MaxValue), tdd.Ending);
	//         case7Sv[nlindex] = CalculationService.CalcTtm(lastYear.Max(), tdd.Ending);
	//         // 6/7
	//         case6S[nlindex] = case6Sv[nlindex] > 30d;
	//         case7S[nlindex] = case7Sv[nlindex] > -25d;
	//
	//
	//         cci[nlindex] = computingData.LineData[LineForm.Cci][i];
	//
	//         cciBs[nlindex] = computingData.LineData[LineForm.Cci][i - 1] switch
	//         {
	//             < -CciIndicate when computingData.LineData[LineForm.Cci][i] > -CciIndicate => Models.CciIndicate
	//                 .BuyTriggered,
	//             > CciIndicate when computingData.LineData[LineForm.Cci][i] < CciIndicate => Models.CciIndicate
	//                 .SellTriggered,
	//             _ => computingData.LineData[LineForm.Cci][i] switch
	//             {
	//                 > 200 => Models.CciIndicate.ExtremelyHigh,
	//                 > 100 => Models.CciIndicate.High,
	//                 < -200 => Models.CciIndicate.ExtremelyLow,
	//                 < -100 => Models.CciIndicate.Low,
	//                 _ => Models.CciIndicate.Normal
	//             }
	//         };
	//
	//         volumes[nlindex] = computingData[i].Volume;
	//
	//         endings[nlindex] = computingData[i].Ending;
	//         opening[nlindex] = computingData[i].Opening;
	//         high[nlindex] = computingData[i].High;
	//         low[nlindex] = computingData[i].Low;
	//         zf[nlindex] = Math.Abs(CalculationService.CalcTtm(low[nlindex], high[nlindex]));
	//         zd[nlindex] = Math.Abs(CalculationService.CalcTtm(computingData[i - 1].Ending, endings[nlindex]));
	//
	//         // case8 unable to catch in this session
	//         nlindex++;
	//     }
	//
	//     bool step1 = false, step2 = false, step3 = false, step4 = false;
	//     var progress0 = 0d;
	//     var progress1 = 0d;
	//     var progress2 = 0d;
	//     var progress3 = 0d;
	//     var progress4 = 0d;
	//
	//     const double ztop = 9.97d;
	//
	//     // store thé index of zhangting time
	//     var zhangTing = new List<int>();
	//     var yiZiBan = new List<int>();
	//
	//     var step1Index = -1;
	//     var step2Index = -1;
	//     var step3Index = -1;
	//     var step4Index = -1;
	//     var tmpIncreame = 0d;
	//
	//     var tmpHigh = 0d;
	//     var tmpLow = 0d;
	//
	//     var readyToBuy = false;
	//     var waitToBuy = false;
	//
	//     var maxVolumeStep1 = 0;
	//
	//     for (var j = 0; j <= endIndex - startIndex; j++)
	//         if (!step1)
	//         {
	//             // when indicator did not start
	//             // ignore any price < 200d line day and etc
	//             if (step1Index < 0)
	//             {
	//                 // 5d line < 200d line, this is bad, we'll ignore all the process
	//                 if (d05[j] < d200[j])
	//                 {
	//                     if (j > 0)
	//                     {
	//                         //TODO reset something
	//                     }
	//
	//                     // reset start
	//                     step1Index = step2Index = step3Index = step4Index = -1;
	//                     tmpIncreame = 0d;
	//                     continue;
	//                 }
	//
	//                 // 5d line < 50d line, this is not good start, we'll ignore all the process
	//                 if (d05[j] < d50[j])
	//                 {
	//                     if (j > 0)
	//                     {
	//                         //TODO reset something
	//                     }
	//
	//                     // reset start
	//                     step1Index = step2Index = step3Index = step4Index = -1;
	//                     tmpIncreame = 0d;
	//                     continue;
	//                 }
	//             }
	//
	//             if (zd[j] < 6) continue;
	//
	//             // record some information
	//             if (zd[j] > ztop)
	//             {
	//                 zhangTing.Add(j);
	//                 if (low[j] > ztop) yiZiBan.Add(j);
	//             }
	//
	//             // init tmp start
	//             if (step1Index < 0)
	//             {
	//                 step1Index = j;
	//                 tmpLow = endings[j];
	//             }
	//
	//             // step 2 still not start
	//             if (step1Index < 0) continue;
	//
	//             step1 = true;
	//         }
	//         else
	//         {
	//             if (!step2)
	//             {
	//                 // step1 =>
	//                 // 1) between 31 trading days, price doubled and more. the more the merrier.
	//                 //    this step might extent to 62 trading day.
	//                 // 2) the 150d will or already higher than 200d.
	//                 // 3) volume 12 times doubled
	//
	//                 if (j - step1Index < 30)
	//                 {
	//                     tmpIncreame += zd[j];
	//                     maxVolumeStep1 = maxVolumeStep1 < volumes[j] ? volumes[j] : maxVolumeStep1;
	//
	//                     tmpHigh = endings[j] > tmpHigh ? endings[j] : tmpHigh;
	//                     tmpLow = endings[j] < tmpHigh ? endings[j] : tmpLow;
	//                 }
	//                 else
	//                 {
	//                     // determine if it is increased 30d
	//                     var s1Zd = CalculationService.CalcTtm(opening[step1Index], high[j]);
	//                     //zd < 90, not we found
	//                     if (s1Zd < 90)
	//                     {
	//                         // reset start
	//                         j = step1Index;
	//                         maxVolumeStep1 = 0;
	//                         step1Index = step2Index = step3Index = step4Index = -1;
	//                         tmpIncreame = 0d;
	//                         step2 = false;
	//                     }
	//                     else // might a good stock
	//                     {
	//                         step2 = true;
	//                         step2Index = j;
	//                     }
	//                 }
	//             }
	//             else
	//             {
	//                 if (!step3)
	//                 {
	//                     // step2 =>
	//                     if (j - step2Index < 25)
	//                     {
	//                         tmpHigh = endings[j] > tmpHigh ? endings[j] : tmpHigh;
	//                         tmpLow = endings[j] < tmpHigh ? endings[j] : tmpLow;
	//                     }
	//                     else
	//                     {
	//                         if (CalculationService.CalcTtm(high[step2Index], tmpLow) < -30
	//                             || CalculationService.CalcTtm(high[step2Index], endings[j]) < -30)
	//                         {
	//                             // reset 
	//                             step2 = false;
	//                             step1 = false;
	//                             maxVolumeStep1 = 0;
	//                             step1Index = step2Index = step3Index = step4Index = -1;
	//                         }
	//                         else
	//                         {
	//                             if (endings[j] > d50[j]) continue;
	//
	//                             // wait for 50d line follow up
	//
	//                             if (!readyToBuy)
	//                                 if (cciBs[j] == Models.CciIndicate.BuyTriggered)
	//                                     readyToBuy = true;
	//
	//                             if (readyToBuy)
	//                             {
	//                                 if (!waitToBuy && zd[j] < 5d)
	//                                 {
	//                                     step2 = true;
	//                                     step3Index = j;
	//                                 }
	//                                 else
	//                                 {
	//                                     if (!waitToBuy) waitToBuy = true;
	//
	//                                     if (!(volumes[j] * 4.7 < maxVolumeStep1)) continue;
	//
	//                                     step2 = true;
	//                                     step3Index = j;
	//                                 }
	//                             }
	//                         }
	//                     }
	//                 }
	//             }
	//         }
	//
	//     return result;
	// }

	#endregion
}