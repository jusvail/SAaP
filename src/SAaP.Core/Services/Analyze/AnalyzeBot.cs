using System.Collections.Generic;
using System.Linq;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze
{
    public class AnalyzeBot
    {
        private readonly IList<OriginalData> _originalData;

        private int _count;

        private int _actualCount;

        private IList<double> _overpricedList;

        private IList<double> _ttm;

        public AnalyzeBot(IList<OriginalData> originalData)
        {
            _originalData = originalData;
            InitialCalc();
        }

        private void InitialCalc()
        {
            _count = _originalData.Count();
            // first day's data is only for calculation so -1
            _actualCount = _count - 1;
            // initialize overprice list
            InitialOverpricedList();
        }

        private void InitialOverpricedList()
        {
            // overpriced list
            _overpricedList = new List<double>(_actualCount);
            // ttm list
            _ttm = new List<double>(_actualCount);

            for (var i = _count - 1; i > 0; i--)
            {
                //calc overprice
                var overprice = CalculationService.CalcOverprice(_originalData[i], _originalData[i - 1]);
                _overpricedList.Add(overprice);

                //calc ttm
                var ttm = CalculationService.CalcTtm(_originalData[i].Ending, _originalData[i - 1].Ending);
                _ttm.Add(ttm);
            }
        }

        public double CalcOverPricedPercent()
        {
            // ReSharper disable once PossibleLossOfFraction
            return CalculationService.Round2(100 * CalcOverPricedDays() / _actualCount);
        }

        public int CalcOverPricedDays()
        {
            return _overpricedList.Count(overprice => overprice > 0);
        }

        public double CalcOverPricedPercentHigherThan1P()
        {
            // ReSharper disable once PossibleLossOfFraction
            return CalculationService.Round2(100 * CalcOverPricedDaysHigherThan1P() / _actualCount);
        }

        public int CalcOverPricedDaysHigherThan1P()
        {
            return _overpricedList.Count(overprice => overprice > 1);
        }

        public int CalcMaxContinueOverPricedDay()
        {
            var days = 0;
            var maxDays = 0;
            foreach (var overprice in _overpricedList)
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
            foreach (var overprice in _overpricedList)
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
            var sl = _overpricedList.Where(overprice => overprice > 0).ToList();
            return sl.Any() ? CalculationService.Round2(sl.Average()) : 0.0;
        }

        public double CalcAverageOverPricedPercentHigherThan1P()
        {
            var sl = _overpricedList.Where(overprice => overprice > 1).ToList();
            return sl.Any() ? CalculationService.Round2(sl.Average()) : 0.0;
        }

        /// <summary>
        /// calculate how much can we earnings if we set a stop profit
        /// </summary>
        /// <param name="stopProfit">1,2,3...etc</param>
        /// <returns>earnings</returns>
        public double CalcStopProfitCompoundInterest(double stopProfit)
        {
            // start amount
            var start = 100.0;

            // loop overprice list
            for (var i = 0; i < _overpricedList.Count; i++)
            {
                // if overprice higher than stop profit, add stop profit only
                if (_overpricedList[i] > stopProfit)
                {
                    start *= (1 + stopProfit / 100);
                }
                // if minus overprice, a loss day :<
                // or a earnings day but not as expected
                else if (_overpricedList[i] < 0)
                {
                    start *= (1 + _ttm[i] / 100);
                }
            }

            // how much can we earned....
            return start;
        }

        public double CalcNoActionProfit()
        {
            var oldest = _originalData[^1].Opening;
            var newest = _originalData[1].Ending;

            return newest * 100 / oldest;
        }

        public double[] CalcBestStopProfitPoint(double upTo)
        {
            // base point
            var stopProfit = 1.0;
            // principal
            var earnings = 100.0;

            // [0]: stop profit
            // [1]: earnings
            var best = new double[2];

            while (stopProfit <= upTo)
            {
                var thisEarning = CalcStopProfitCompoundInterest(stopProfit);

                if (thisEarning > earnings)
                {
                    earnings = thisEarning;
                    best[0] = stopProfit;
                    best[1] = thisEarning;
                }

                // 0.1 each loop
                stopProfit += 0.1;
            }

            return best;
        }

        public string CalcEvaluate()
        {
            return CalcNoActionProfit() switch
            {
                > 200 => "躺赚{p%100}倍",
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
