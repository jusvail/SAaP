using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Models
{
    public class AnalysisResult
    {
        public string CodeName { get; set; }

        public string CompanyName { get; set; }

        public int Duration { get; set; }

        public double OverPricedPercent { get; set; }

        public int OverPricedDays { get; set; }

        public double OverPricedPercentHigherThan1P { get; set; }

        public int OverPricedDaysHigherThan1P { get; set; }

        public int MaxContinueOverPricedDay { get; set; }

        public double MaxContinueMinusOverPricedDay { get; set; }

        public double AverageOverPricedPercent { get; set; }

        public double AverageOverPricedPercentHigherThan1P { get; set; }

        public string Evaluate { get; set; }
    }
}
