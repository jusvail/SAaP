using System;
using System.Collections.Generic;
using System.Linq;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyze
{
    public class AnalyzeBot
    {
        private readonly IList<OriginalData> _originalData;

        private int _count;

        public int ActualCount;

        public IList<double> OverpricedList;

        public IList<double> Ttm;

        public IList<double> Amplitude;

        public AnalyzeBot(IList<OriginalData> originalData)
        {
            _originalData = originalData;
            InitialCalc();
            InitialList();
        }

        private void InitialCalc()
        {
            _count = _originalData.Count;
            // first day's data is only for calculation so -1
            ActualCount = _count - 1;
        }

        private void InitialList()
        {
	        if (ActualCount< 0)
	        {
		        return;
	        }
            // overpriced list
            OverpricedList = new List<double>(ActualCount);
            // ttm list
            Ttm = new List<double>(ActualCount);

            Amplitude = new List<double>(ActualCount);

            for (var i = _count - 1; i > 0; i--)
            {
                //calc overprice
                var overprice = CalculationService.CalcOverprice(_originalData[i], _originalData[i - 1]);
                OverpricedList.Add(overprice);

                //calc ttm
                var ttm = CalculationService.CalcTtm(_originalData[i].Ending, _originalData[i - 1].Ending);
                Ttm.Add(ttm);

                var amplitude = CalculationService.CalcTtm(_originalData[i].Low, _originalData[i].High);
                Amplitude.Add(amplitude);
            }
        }

        public double CalcMinimalAmplitude()
        {
            return Amplitude.Min();
        }

        public double CalcMaxAmplitude()
        {
            return Amplitude.Max();
        }

        public double CalcMedianAmplitude()
        {
            var tp = new List<double>();

            tp.AddRange(Amplitude);
            tp.Sort();

            return Amplitude[tp.Count / 2];
        }

        public double CalcAverageAmplitude()
        {
            // ReSharper disable once PossibleLossOfFraction
            return CalculationService.Round2(Amplitude.Average());
        }

        public double CalcOverPricedPercent()
        {
            // ReSharper disable once PossibleLossOfFraction
            return CalculationService.Round2(100 * CalcOverPricedDays() / ActualCount);
        }

        public int CalcOverPricedDays()
        {
            return OverpricedList.Count(overprice => overprice > 0);
        }

        public double CalcOverPricedPercentHigherThan1P()
        {
            // ReSharper disable once PossibleLossOfFraction
            return CalculationService.Round2(100 * CalcOverPricedDaysHigherThan1P() / ActualCount);
        }

        public int CalcOverPricedDaysHigherThan1P()
        {
            return OverpricedList.Count(overprice => overprice > 1);
        }

        public int CalcMaxContinueOverPricedDay()
        {
            var days = 0;
            var maxDays = 0;
            foreach (var overprice in OverpricedList)
            {
                if (overprice > 0)
                {
                    days++;
                }
                else
                {
                    if (days > maxDays) maxDays = days;
                    days = 0;
                }
            }
            return maxDays > days ? maxDays : days;
        }

        public double CalcMaxContinueMinusOverPricedDay()
        {
            var days = 0;
            var maxDays = 0;
            foreach (var overprice in OverpricedList)
            {
                if (overprice < 0)
                {
                    days++;
                }
                else
                {
                    if (days > maxDays) maxDays = days;
                    days = 0;
                }
            }
            return maxDays > days ? maxDays : days;
        }

        public double CalcAverageOverPricedPercent()
        {
            var sl = OverpricedList.Where(overprice => overprice > 0).ToList();
            return sl.Any() ? CalculationService.Round2(sl.Average()) : 0.0;
        }

        public double CalcAverageOverPricedPercentHigherThan1P()
        {
            var sl = OverpricedList.Where(overprice => overprice > 1).ToList();
            return sl.Any() ? CalculationService.Round2(sl.Average()) : 0.0;
        }

        private void CalcReasonablePrincipal(out double principal, out double divisor)
        {
            switch (_originalData.Select(o => o.High).Max())
            {
                case > 10000:
                    principal = 10000000.0;
                    divisor = 100000;
                    break;
                case > 1000:
                    principal = 1000000.0;
                    divisor = 10000;
                    break;
                default:
                    principal = 100000.0;
                    divisor = 1000;
                    break;
            }
        }

        /// <summary>
        /// calculate how much can we earnings if we set a stop profit
        /// </summary>
        /// <param name="stopProfit">1,2,3...etc</param>
        /// <returns>earnings</returns>
        public double CalcStopProfitCompoundInterest(double stopProfit)
        {
            // start amount
            CalcReasonablePrincipal(out var principal, out var divider);

            // hold stock / how much we can buy at ending of first day's yesterday
            // 1 hand => 100 stock
            var holdHand = Math.Floor(principal / (_originalData[^1].Ending * 100));
            // remind principal
            var remind = principal - holdHand * 100 * _originalData[^1].Ending;

            // assume buy at 0 day's ending

            // loop overprice list
            for (var i = _originalData.Count - 2; i >= 0; i--)
            {
                // before day's ending
                var yesterdaysEnding = _originalData[i + 1].Ending;

                // if overprice higher than stop profit, add stop profit only
                if (OverpricedList[_originalData.Count - i - 2] >= stopProfit)
                {
                    // sold all stock
                    var sold = yesterdaysEnding * (1 + stopProfit / 100) * holdHand * 100;
                    holdHand = 0;
                    // a goo day :> hahaha
                    principal = remind + sold;

                    // not last of analyze day
                    if (i == OverpricedList.Count - 1) break;

                    // today's ending, we buy again
                    holdHand = Math.Floor(principal / (_originalData[i].Ending * 100));
                    remind = principal - holdHand * 100 * _originalData[i].Ending;
                }
                // if minus overprice, a loss day :< f**k
                // or a earnings day but not as expected
                else
                {
                    // do nothing
                }
            }

            // if we have remind stock, sold all
            if (holdHand != 0)
            {
                principal = _originalData[0].Ending * holdHand * 100 + remind;
            }

            // how much can we earned....
            return principal / divider;
        }

        public double CalcNoActionProfit()
        {
            var oldest = _originalData[^1].Ending;
            var newest = _originalData[0].Ending;

            return newest * 100 / oldest;
        }

        public double[] CalcBestStopProfitPoint(double upTo)
        {
            // base point
            var stopProfit = 1.0;
            var bestSp = new List<double>();
            var bestEarning = new List<double>();

            while (stopProfit <= upTo)
            {
                bestSp.Add(stopProfit);
                bestEarning.Add(CalcStopProfitCompoundInterest(stopProfit));

                // 0.1 each loop
                stopProfit += 0.1;
            }

            var max = bestEarning.Max();
            var index = bestEarning.IndexOf(max);

            // [0]: stop profit
            // [1]: earnings
            return new[] { bestSp[index], max };
        }

        public string CalcEvaluate()
        {
            var earn = CalcNoActionProfit();
            return earn switch
            {
                > 200 => $"躺赚{CalculationService.Round2(earn / 100)}倍",
                > 150 => "躺赚50%+",
                > 120 => "躺赚20%+",
                > 110 => "躺赚10%+",
                > 100 => "喝点鸡汤",
                > 90 => "随时准备止损",
                > 80 => "放手吧",
                _ => "我小叮当觉得垃圾"
            };
        }
    }
}
