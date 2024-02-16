using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Analyze;
using SAaP.Core.Services.Generic;
using SAaP.Models;

namespace SAaP.Services;

/// <summary>
///     Simple stock analyze service
/// </summary>
public class StockAnalyzeService : IStockAnalyzeService
{
	/// <summary>
	///     main analyze method
	/// </summary>
	/// <param name="codeName">stock code name</param>
	/// <param name="duration">duration(from last trading day)</param>
	/// <returns></returns>
	public async Task<AnalysisResultDetail> AnalyzeAsync(string codeName, int duration)
	{
		var fetchStockDataService = App.GetService<IFetchStockDataService>();
		var belong = await fetchStockDataService.TryGetBelongByCode(codeName);

		var codeMain = StockService.CutStockCodeLen7ToLen6(codeName);

		// query original data recently
		var originalData = await DbService.TakeOriginalDataFromFile(codeMain, belong, duration);

		// return if no any record
		// New stock cannot analyze
		if (originalData == null || !originalData.Any() || originalData.Count < 2) return null;

		// main analyze process
		var bot = new AnalyzeBot(originalData);

		// best stop profit calculate
		// up to 7%
		const double upTo = 9.9;
		var best = bot.CalcBestStopProfitPoint(upTo);

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
				AverageAmplitude = bot.CalcAverageAmplitude(),
				MedianAmplitude = bot.CalcMedianAmplitude(),
				MinimalAmplitude = bot.CalcMinimalAmplitude(),
				MaxAmplitude = bot.CalcMaxAmplitude(),
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
			Console.WriteLine(GetType());
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
			if (l[i] * r[i] > 0)
				d++;

		return $"正相关性： {Math.Round(d * 100.0 / c, 2)}%";
	}

	public async IAsyncEnumerable<Stock> Filter(IEnumerable<Stock> stocks,
												List<ObservableTrackCondition> trackConditions, DateTimeOffset lastTradingDate,
												[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		if (!trackConditions.Any()) yield break;

		var conditions = trackConditions.SelectMany(trackCondition => Condition.Parse(trackCondition.TrackContent))
										.ToList();

		if (!conditions.Any()) yield break;

		// var duration = conditions.Max(c => c.FromDays);

		foreach (var stock in stocks)
		{
			if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

			var filter = new CodeFilter
			{
				CodeName = stock.CodeName,
				TrackCondition = conditions,
				BelongTo = stock.BelongTo,
				LastTradingDay = lastTradingDate.DateTime
			};

			if (await filter.FilterAll() && !stock.CompanyName.Contains("退"))
				yield return stock;
			else
				yield return null;
		}
	}

	public async Task<BuySimulateReport> Simulate(SimulateConfiguration configuration, CancellationToken cancellationToken)
	{
		var report = new BuySimulateReport();

		var trackConditions = configuration.TaskDetail.TrackConditions;

		if (!trackConditions.Any()) return report;

		try
		{
			var conditions = trackConditions.SelectMany(trackCondition => Condition.Parse(trackCondition.TrackContent))
											.ToList();

			var stocks = new List<Stock>();

			if (configuration.Stocks.Any())
			{
				stocks = configuration.Stocks;
			}
			else
			{
				var asyncStocks = App.GetService<IDbTransferService>().SelectAllLocalStoredCodes();
				await foreach (var stock in asyncStocks.WithCancellation(cancellationToken)) stocks.Add(stock);
			}


#if MONITORPAGEONLY
			// stocks.Clear();
			// stocks.Add(new Stock
			// {
			// 	BelongTo = 0,
			// 	CodeName = "002992",
			// 	CompanyName = "宝宝科技"
			// });
#endif

			var arg = new NotifyUserEventArgs();

			// stocks = stocks.Where(stock => !stock.CompanyName.Contains("退") && !stock.CompanyName.Contains("ST")).ToList();
			stocks = stocks.Where(stock => !stock.CompanyName.Contains("退")).ToList();
			var allCount = stocks.Count;

			for (var si = 0; si < stocks.Count; si++)
			{
				if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

				try
				{
					var stock = stocks[si];
					arg.Message = $"正在分析: {stock.CompanyName} 进度: {si + 1}/{allCount}";

					configuration.OnOnAnalyzeCallback(arg);

					var ori = await DbService.TakeOriginalDataAscending(stock.CodeName, stock.BelongTo);

					if (!ori.Any()) continue;

					var ids = -1;
					var ide = -1;
					var cur = ori.Count - 1;
					while (ids < 0 || ide < 0)
					{
						if (ids < 0 && DateTime.Parse(ori[cur].Day) < configuration.StartDate) ids = cur;
						if (ide < 0 && DateTime.Parse(ori[cur].Day) < configuration.EndDate) ide = cur;
						cur--;
						if (cur <= 0) break;
					}

					if (cur <= 0) continue;

					for (var i = ids; i <= ide; i++)
					{
						var lastDay = DateTime.Parse(ori[i].Day);
						var filter = new CodeFilter
						{
							CodeName = stock.CodeName,
							TrackCondition = conditions,
							BelongTo = stock.BelongTo,
							LastTradingDay = lastDay
						};

						var tsk = await filter.FilterAll();
						if (!tsk) continue;

						// found!
						var buy = ori[i];

						const int range = 30;
						// 向后找30交易日或最后一日
						var min30 = Math.Min(i + range, ori.Count - 1);
						var min60 = Math.Min(i + range * 2, ori.Count - 1);

						if (min30 == ori.Count - 1) continue;

						var result = new SimulateResult
						{
							BuyDate = buy,
							// 30 && 60 日涨幅
							D30Profit = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, ori[min30].Ending)),
							D60Profit = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, ori[min60].Ending))
						};

						var j = i + 1;
						var max = ori[j].Ending;
						var maxIndex = j;
						for (; j <= min30; j++)
						{
							if (ori[j].Ending < max) continue;

							maxIndex = j;
							max = ori[j].Ending;
						}

						result.D30PassingDay = maxIndex - i;
						result.D30Profit = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, max));
						result.D30Success = result.D30Profit > 0d;
						result.D30SellDate = ori[maxIndex];

						var k = i + 1;
						var low = ori[i].Ending;
						// var lowIndex = k;
						for (; k < min30; k++)
						{
							if (ori[k].Ending > low) continue;

							// lowIndex = k;
							low = ori[k].Ending;
						}

						var pb30 = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, low));
						result.D30Pullback = pb30 < 0 ? pb30 : 0;

						// -----------------------------------------------------------------------
						// 60d 计算
						// -----------------------------------------------------------------------
						j = min30 + 1;
						for (; j <= min60; j++)
						{
							if (ori[j].Ending < max) continue;

							maxIndex = j;
							max = ori[j].Ending;
						}

						result.D60PassingDay = maxIndex - i;
						result.D60Profit = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, max));
						result.D60Success = result.D30Profit > 0d;
						result.D60SellDate = ori[maxIndex];

						k = min30 + 1;
						// var lowIndex = k;
						for (; k < min30; k++)
						{
							if (ori[k].Ending > low) continue;

							// lowIndex = k;
							low = ori[k].Ending;
						}

						var pb60 = CalculationService.Round2(CalculationService.CalcTtm(ori[i + 1].Opening, low));
						result.D60Pullback = pb60 < 0d ? pb60 : 0d;

						result.CodeName = stock.CodeNameFull;
						result.CompanyName = stock.CompanyName;

						i = min60;
						report.SimulateResults.Add(result);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine(nameof(GetType));
				}
			}
		}
		catch (TaskCanceledException)
		{
			return report;
		}
		catch (Exception)
		{
			return report;
		}

		var resultCount = report.SimulateResults.Count;

		if (resultCount <= 0) return report;

		var successCountD30 = report.SimulateResults.Count(r => r.D30Success);
		var successCountD60 = report.SimulateResults.Count(r => r.D60Success);
		var successCount = report.SimulateResults.Count(r => r.D30Success || r.D60Success);

		report.ReportSummary.OverallAccuracyRate = CalculationService.Round2(100 * successCount / resultCount).ToString();
		report.ReportSummary.D30AccuracyRate = CalculationService.Round2(100 * successCountD30 / resultCount).ToString();
		report.ReportSummary.D30AccuracyProfit = CalculationService.Round2(report.SimulateResults.Sum(r => r.D30Profit) / resultCount).ToString();
		report.ReportSummary.D30AccuracyPullback = CalculationService.Round2(report.SimulateResults.Sum(r => r.D30Pullback) / resultCount).ToString();
		report.ReportSummary.D60AccuracyRate = CalculationService.Round2(100 * successCountD60 / resultCount).ToString();
		report.ReportSummary.D60AccuracyProfit = CalculationService.Round2(report.SimulateResults.Sum(r => r.D60Profit) / resultCount).ToString();
		report.ReportSummary.D60AccuracyPullback = CalculationService.Round2(report.SimulateResults.Sum(r => r.D60Pullback) / resultCount).ToString();

		return report;
	}

	public async Task<StatisticsReport> Statistics(StatisticsConfiguration configuration, CancellationToken cancellationToken)
	{
		var report = new StatisticsReport();
		var task = configuration.TaskDetail;
		report.TaskDetail = task;

		var arg = new NotifyUserEventArgs();

		var allCount = configuration.Stocks.Count;

		for (var si = 0; si < allCount; si++)
		{
			var stock = configuration.Stocks[si];
			if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

			arg.Message = $"正在分析: {stock.CompanyName} 进度: {si + 1}/{allCount}";

			try
			{
				var ori = await DbService.TakeOriginalDataAscending(stock.CodeName, stock.BelongTo,
																	task.StartDate.LocalDateTime,
																	task.EndDate.LocalDateTime);

				configuration.OnOnAnalyzeCallback(arg);

				if (ori == null || ori.Count < 200) continue;

				var high = ori.First().Ending;
				var highIndex = 0;

				var low = ori.First().Opening;
				var lowIndex = 0;

				for (var j = 0; j < ori.Count; j++)
				{
					var t = ori[j];
					if (t.Ending > high)
					{
						high = t.Ending;
						highIndex = j;
					}

					if (t.Opening < low)
					{
						low = t.Opening;
						lowIndex = j;
					}
				}

				var lowH = ori[highIndex].Opening;
				var lowHIndex = highIndex;

				for (var j = highIndex; j >= 0; j--)
				{
					var t = ori[j];
					if (t.Opening < lowH)
					{
						lowH = t.Opening;
						lowHIndex = j;
					}
				}

				var highL = ori[lowIndex].Ending;
				var highLIndex = lowIndex;

				for (var j = lowIndex; j <= highIndex; j++)
				{
					var t = ori[j];
					if (t.Ending > highL)
					{
						highL = t.Ending;
						highLIndex = j;
					}
				}

				var zdFromH = CalculationService.CalcTtm(lowH, high);
				var zdFromL = CalculationService.CalcTtm(low, highL);

				if (Math.Abs(zdFromH - zdFromL) > 0.01)
				{
					if (zdFromH > zdFromL)
					{
						low = lowH;
						lowIndex = lowHIndex;
					}
					else
					{
						high = highL;
						highIndex = highLIndex;
					}
				}

				var maxZd = CalculationService.CalcTtm(low, high);

				// 根据前期上涨的幅度循环，从大到小
				for (var pullUp = task.PullUpBeforeEnd; pullUp >= task.PullUpBeforeStart; pullUp -= task.StepLength)
					if (maxZd > pullUp)
					{
						// 最大涨跌>当前计算涨跌，可以继续
						var buyIndex = -1;
						var buyPrice = 0d;

						var cur = lowIndex;
						while (cur <= highIndex)
						{
							var tzd = CalculationService.CalcTtm(low, ori[cur].Ending);

							if (tzd > pullUp)
							{
								buyIndex = cur;
								buyPrice = ori[cur].Ending;
								break;
							}

							cur++;
						}

						if (buyIndex < 0) continue;

						// while (cur <= highIndex)
						// {
						// 	sellIndex = cur;
						// 	if (ori[cur].Ending >= sellPrice)
						// 	{
						// 		sellPrice = ori[cur].Ending;
						// 		break;
						// 	}
						//
						// 	cur++;
						// }
						var sellIndex = highIndex;
						var sellPrice = ori[highIndex].Ending;

						var profit = CalculationService.CalcTtm(buyPrice, sellPrice);

						// 总结
						var statistics = new StatisticsResult
						{
							BuyDate = ori[buyIndex],
							SellDate = ori[sellIndex],
							Stock = stock,
							PullUpBefore = pullUp,
							Profit = profit
						};

						var pullBackPrice = buyPrice;
						var pullBackIndex = buyIndex;

						for (var i = buyIndex; i <= sellIndex; i++)
							if (pullBackPrice > ori[i].Ending)
							{
								pullBackPrice = ori[i].Ending;
								pullBackIndex = i;
							}

						statistics.IsSuccess = profit > task.ExpectedProfit;
						// 预期达成总结
						statistics.MaxPullBack = CalculationService.CalcTtm(buyPrice, pullBackPrice);
						statistics.MaxPullBackDate = ori[pullBackIndex];
						statistics.DaysOfBuyToSell = sellIndex - buyIndex + 1;
						statistics.DaysOfBuyToMaxPullBack = pullBackIndex - buyIndex + 1;

						if (report.StatisticsResults.TryGetValue(pullUp, out var statisticsCollection))
							statisticsCollection.Add(statistics);
						else
							report.StatisticsResults.Add(pullUp, new ObservableCollection<StatisticsResult> { statistics });
					}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		return report;
	}
}