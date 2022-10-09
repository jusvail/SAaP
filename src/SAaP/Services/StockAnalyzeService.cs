using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Mapster;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using SAaP.Core.Services.Analyze;
using SAaP.Models;

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
    /// <returns></returns>
    public async Task<AnalysisResultDetail> Analyze(string codeName, int duration)
    {
        var fetchStockDataService = App.GetService<IFetchStockDataService>();
        var belong = await fetchStockDataService.TryGetBelongByCode(codeName);

        var codeMain = StockService.CutStockCodeToSix(codeName);

        // query original data recently
        var originalData = await DbService.TakeOriginalData(codeMain, belong, duration);

        // return if no any record
        // New stock cannot analyze
        if (!originalData.Any() || originalData.Count < 2) return null;

        // main analyze process
        var bot = new AnalyzeBot(originalData);

        // best stop profit calculate
        // up to 7%
        var best = bot.CalcBestStopProfitPoint(7.0);

        // select company Name
        var companyName = await DbService.SelectCompanyNameByCode(codeMain, belong);

        try
        {
            // analyze result store into object
            var analyzeResult = new AnalysisResultDetail
            {
                Duration = duration,
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
            return analyzeResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public string CalcRelationPercent(IList<IList<double>> compare)
    {
        if (compare.Count != 2) return string.Empty;

        var l = compare[0];
        var r = compare[1];

        if (l.Count != r.Count) return string.Empty;

        double c = l.Count;
        var d = 0.0;

        for (var i = 0; i < l.Count; i++)
        {
            if (l[i] * r[i] > 0)
            {
                d++;
            }
        }

        return $"正相关性： {Math.Round(d * 100.0 / c, 2)}%";
    }

    public async IAsyncEnumerable<string> Filter(IEnumerable<string> codeNames,
        List<ObservableTrackCondition> trackConditions, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!trackConditions.Any())
        {
            yield return string.Empty;
            yield break;
        }

        var conditions = trackConditions.Select(condition => Condition.Parse(condition.TrackValue)).ToList();

        const int duration = 10;

        foreach (var codeName in codeNames)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            // query original data recently
            var originalData = await DbService.TakeOriginalData(codeName[1..], Convert.ToInt32(codeName[..1]), duration);

            var filter = new CodeFilter
            {
                CodeName = codeName,
                TrackCondition = conditions,
                OriginalDatas = originalData
            };

            if (filter.Filter())
            {
                yield return codeName;
            }
            else
            {
                yield return string.Empty;
            }
        }
    }
}