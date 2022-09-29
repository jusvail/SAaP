using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB
{
    public enum TradeType
    {
        Buy = 0,
        Sell = 1,
        Unknown = 2
    }

    [Table(Name = "InvestData")]
    public class InvestData
    {
        [Column]
        public int TradeIndex { get; set; }

        [Column]
        public string CodeName { get; set; }

        [Column]
        public string CompanyName { get; set; }

        [Column]
        public DateTime TradeDate { get; set; }

        [Column]
        public string TradeTime { get; set; }

        [Column]
        public TradeType TradeType { get; set; }

        [Column]
        public int Volume { get; set; }

        [Column]
        public double Price { get; set; }
    }
}