using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB
{
    [Table(Name = "InvestSummaryData")]
    public class InvestSummaryData
    {
        [Column]
        [PrimaryKey, Identity]
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
        public double AverageSell { get; set; }

        [Column]
        public int Volume { get; set; }

        [Column]
        public double Profit { get; set; }

        [Column]
        public bool IsArchived { get; set; }
    }
}
