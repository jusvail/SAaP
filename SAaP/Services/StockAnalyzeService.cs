using System;
using System.Collections.Generic;
using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Models;
using SAaP.Core.Services;
using SAaP.Core.Services.Analyze;

namespace SAaP.Services
{
    /// <summary>
    /// Simple stock analyze service
    /// </summary>
    internal class StockAnalyzeService : IStockAnalyzeService
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

            // analyze result store into object
            var analyzeResult = new AnalysisResult
            {
                CodeName = codeName,
                CompanyName = "没接口！", // TODO add companyName acquire service
                OverPricedPercent = bot.CalcOverPricedPercent(),
                OverPricedDays = bot.CalcOverPricedDays(),
                OverPricedPercentHigherThan1P = bot.CalcOverPricedPercentHigherThan1P(),
                OverPricedDaysHigherThan1P = bot.CalcOverPricedDaysHigherThan1P(),
                MaxContinueOverPricedDay = bot.CalcMaxContinueOverPricedDay(),
                MaxContinueMinusOverPricedDay = bot.CalcMaxContinueMinusOverPricedDay(),
                AverageOverPricedPercent = bot.CalcAverageOverPricedPercent(),
                AverageOverPricedPercentHigherThan1P = bot.CalcAverageOverPricedPercentHigherThan1P(),
                Evaluate = bot.CalcEvaluate()
            };

            // callback invocation
            callback?.Invoke(analyzeResult);
        }
    }
}
