namespace SAaP.Core.Models;

public class AnalysisResult
{
    public string CodeName { get; set; }

    public string CompanyName { get; set; }

    public double OverPricedPercent { get; set; }

    public int OverPricedDays { get; set; }

    public double OverPricedPercentHigherThan1P { get; set; }

    public int OverPricedDaysHigherThan1P { get; set; }

    public int MaxContinueOverPricedDay { get; set; }

    public double MaxContinueMinusOverPricedDay { get; set; }

    public double AverageOverPricedPercent { get; set; }

    public double AverageOverPricedPercentHigherThan1P { get; set; }

    public double StopProfitWith1P { get; set; }

    public double StopProfitWith2P { get; set; }

    public double StopProfitWith3P { get; set; }

    public double NoActionProfit { get; set; }

    public double BestStopProfit { get; set; }

    public double BestEarnings { get; set; }

    public double AverageAmplitude { get; set; }

    public double MedianAmplitude { get; set; }

    public double MinimalAmplitude { get; set; }

    public double MaxAmplitude { get; set; }

    public string FirstTradingDay { get; set; }

    public string LastTradingDay { get; set; }

    public string Evaluate { get; set; }
}