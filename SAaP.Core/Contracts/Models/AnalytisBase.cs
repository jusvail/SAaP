using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Contracts.Models
{
    public class AnalytisBase
    {
        public string codeName { get; set; }

        public string companyName { get; set; }

        public double overPricedDays { get; set; }

        public double m_overPricedDays { get; set; }
            
        public double overPricedDaysHigherThan1P { get; set; }

        public double m_overPricedDaysHigherThan1P { get; set; }


    }
}
