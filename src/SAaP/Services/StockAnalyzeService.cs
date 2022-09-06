using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Models;
using SAaP.Core.Services;
using SAaP.Core.Services.Analyze;

namespace SAaP.Services;

/// <summary>
/// Simple stock analyze service
/// </summary>
public class StockAnalyzeService : IStockAnalyzeService
{
    /// <summary>
    /// main analyze method
    /// </summary>
    /// <param name="codeName">stock code name</param>
    /// <param name="duration">duration(from last trading day)</param>
    /// <param name="callback">callback method</param>
    /// <returns></returns>
    public async Task Analyze(string codeName, int duration, Action<AnalysisResult> callback)
    {
        // initial db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // query original data recently [duration]
        var originalData = (from data in db.OriginalData
            where data.CodeName == codeName
            orderby data.Day descending
            select data).Take(duration + 1).ToList(); // +1 cause ... u know y

        // return if no any record
        if (!originalData.Any()) return;

        // main analyze process
        var bot = new AnalyzeBot(originalData);

        // best stop profit calculate
        // up to 6%
        var best = bot.CalcBestStopProfitPoint(6.0);

        // select company Name
        var companyName = await DbService.SelectCompanyNameByCode(codeName);

        // analyze result store into object
        var analyzeResult = new AnalysisResult
        {
            CodeName = codeName,
            CompanyName = companyName,
            OverPricedPercent = bot.CalcOverPricedPercent(),
            OverPricedDays = bot.CalcOverPricedDays(),
            OverPricedPercentHigherThan1P = bot.CalcOverPricedPercentHigherThan1P(),
            OverPricedDaysHigherThan1P = bot.CalcOverPricedDaysHigherThan1P(),
            MaxContinueOverPricedDay = bot.CalcMaxContinueOverPricedDay(),
            MaxContinueMinusOverPricedDay = bot.CalcMaxContinueMinusOverPricedDay(),
            AverageOverPricedPercent = bot.CalcAverageOverPricedPercent(),
            AverageOverPricedPercentHigherThan1P = bot.CalcAverageOverPricedPercentHigherThan1P(),
            StopProfitWith1P = bot.CalcStopProfitCompoundInterest(1),
            StopProfitWith2P = bot.CalcStopProfitCompoundInterest(2),
            StopProfitWith3P = bot.CalcStopProfitCompoundInterest(3),
            NoActionProfit = bot.CalcNoActionProfit(),
            BestStopProfit = best[0],
            BestEarnings = best[1],
            FirstTradingDay = originalData[^2].Day,
            LastTradingDay = originalData[0].Day,
            Evaluate = bot.CalcEvaluate()
        };

        // callback invocation
        callback?.Invoke(analyzeResult);
    }
}