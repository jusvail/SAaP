using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB
{
    [Table(Name = "InvestData")]
    public class InvestSummaryData
    {
        [Column]
        public int TradeIndex { get; set; }

        [Column]
        public string CodeName { get; set; }

        [Column]
        public string CompanyName { get; set; }

        [Column]
        public DateTime Start { get; set; }

        [Column]
        public DateTime End { get; set; }

        [Column]
        public double AverageCost { get; set; }

        [Column]
        public double AverageSale { get; set; }

        [Column]
        public double Profit { get; set; }

        [Column]
        public bool IsArchived { get; set; }
    }
}
