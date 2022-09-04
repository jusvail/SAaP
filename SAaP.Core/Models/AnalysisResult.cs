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

        public double OverPricedPercent { get; set; }

        public double OverPricedDays { get; set; }

        public double OverPricedPercentHigherThan1P { get; set; }

        public double OverPricedDaysHigherThan1P { get; set; }

        public double MaxContinueOverPricedDay { get; set; }

        public double MaxContinueMinusOverPricedDay { get; set; }

        public double AverageOverPricedPercent { get; set; }

        public double AverageOverPricedPercentHigherThan1P { get; set; }

        public string Evaluate { get; set; }
    }
}
