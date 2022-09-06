using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "AnalyzedData")]
public class AnalyzedData
{
    [PrimaryKey]
    public string CodeName { get; set; }

    [PrimaryKey]
    public DateTime AnalyzeDate { get; set; }

    [Column]
    public int LastXTradingDay { get; set; }

    [Column]
    public string CompanyName { get; set; }

    [Column]
    public double OverPricedPercent { get; set; }

    [Column]
    public double OverPricedDays { get; set; }

    [Column]
    public double OverPricedPercentHigherThan1P { get; set; }

    [Column]
    public double OverPricedDaysHigherThan1P { get; set; }

    [Column]
    public double MaxContinueOverPricedDay { get; set; }

    [Column]
    public double MaxContinueMinusOverPricedDay { get; set; }

    [Column]
    public double AverageOverPricedPercent { get; set; }

    [Column]
    public double AverageOverPricedPercentHigherThan1P { get; set; }

    [Column]
    public string Evaluate { get; set; }
}