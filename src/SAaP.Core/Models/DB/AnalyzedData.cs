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

    [PrimaryKey]
    public int LastXTradingDay { get; set; }

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