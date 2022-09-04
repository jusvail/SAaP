using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Analyze
{
    public class AnalyzeBot
    {
        private readonly IList<OriginalData> _originalData;

        private int _count;

        private int _actualCount;

        private IList<double> _overpricedList;

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

            for (var i = _count - 1; i > 0; i--)
            {
                //calc overprice
                var overprice = CalculationService.CalcOverprice(_originalData[i], _originalData[i - 1]);
                _overpricedList.Add(overprice);
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
            return maxDays;
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
            return maxDays;
        }

        public double CalcAverageOverPricedPercent()
        {
           return _overpricedList.Where(overprice => overprice > 0).ToList().Average();
        }

        public double CalcAverageOverPricedPercentHigherThan1P()
        {
            return _overpricedList.Where(overprice => overprice > 1).ToList().Average();
        }

        public string CalcEvaluate()
        {
            return CalcOverPricedPercentHigherThan1P() > 90 ? "我小叮当觉得很牛" : "BOT我不造啊！";
        }
    }
}
