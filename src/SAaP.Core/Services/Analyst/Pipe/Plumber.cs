using System;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyst.Pipe;

internal class Plumber
{
	public Plumber()
	{
	}

	public Plumber(PatternType type)
	{
		PatternType = type;
	}

	public PatternType PatternType { get; set; }

	public void PreProcess(ComputingData computingData)
	{
		for (var i = 0; i < computingData.HistoricDataCount; i++)
		{
			// calculate X day line
			foreach (var lineForm in LineData.FormOfDayLine.Keys)
			{
				var need = LineData.FormOfDayLine[lineForm];

				// -1 cause list start at 0
				if (i >= need - 1)
				{
					// calculate X day line
					var av5 = PlumbersWife.CalculateAverageDayLine(computingData.OriginalDatas, i, need);
					// add to overall datalist
					computingData.LineData[lineForm].Add(av5);
				}
				else
				{
					// add minus data to filling the array
					computingData.LineData[lineForm].Add(double.MinValue);
				}
			}

			#region rps unable to calculate according to single stock data

			// calculate X rps line
			// foreach (var rpsForm in LineData.FormOfRpsLine.Keys)
			// {
			//     // -1 cause list start at 0
			//     if (i >= LineData.FormOfRpsLine[rpsForm] - 1)
			//     {
			//         // calculate X rps line
			//         var av5 = PlumbersWife.CalculateRpsLine(computingData.OriginalDatas, i, rpsForm);
			//         // add to overall datalist
			//         computingData.LineData[rpsForm].Add(av5);
			//     }
			//     else
			//     {
			//         // add minus data to filling the array
			//         computingData.LineData[rpsForm].Add(double.MinValue);
			//     }
			// }

			#endregion
		}

		//calculate cci line
		// 使用参数5!
		computingData.LineData[LineForm.Cci] = PlumbersWife.CalculateCciLine(computingData.OriginalDatas, 5);
	}

	public void IndicatePattern(ComputingData computingData)
	{
		var dataIndex = computingData.OriginalDatas.Count - 1;

		var patternIndicator = PatternIndicator.Create(PatternType);

		var startIndex = dataIndex - 300 + 1;

#if DEBUG
		if (startIndex < 0)
		{
			Console.WriteLine(startIndex);
		}
#endif

		computingData.MatchResult =
			startIndex >= 0
				? patternIndicator.InitField(computingData, dataIndex - 300 + 1, dataIndex).Indicate()
				: MatchResult.Empty;
	}

	public Report GenerateReport(ComputingData computingData)
	{
		var report = new Report();

		var result = computingData.MatchResult;

		report.CodeName = computingData.Stock.CodeNameFull;
		report.CompanyName = computingData.Stock.CompanyName;

		if (result.Error)
		{
			report.CurrentStep = "Error Happened";
			return report;
		}

		if (result.Step4Found) report.CurrentStep = nameof(result.Step4Found);
		else if (result.Step3Found) report.CurrentStep = nameof(result.Step3Found);
		else if (result.Step2Found) report.CurrentStep = nameof(result.Step2Found);
		else if (result.Step1Found) report.CurrentStep = nameof(result.Step1Found);
		else report.CurrentStep = "No Found";

		if (result.BuyProgress > 0) report.Step2Progress = $"进度：{CalculationService.Round2(result.BuyProgress)}/100";

		if (result.Bought)
		{
			report.BuyAt = result.BoughtDay;
			report.BuyPrice = result.BuyPrice + "";
		}

		if (result.Sold)
		{
			report.SellAt = result.SoldDay;
			report.Profit = result.Profit;
			report.SellPrice = result.SellPrice + "";
			report.HoldingDays = result.HoldingDays + "D";
		}

		if (result.Step1Found) report.Step1 = result.Message.Count >= 1 ? result.Message[0] : string.Empty;
		if (result.Step2Found) report.Step2 = result.Message.Count >= 2 ? result.Message[1] : string.Empty;
		if (result.Step3Found) report.Step3 = result.Message.Count >= 3 ? result.Message[2] : string.Empty;
		if (result.Step4Found) report.Step4 = result.Message.Count >= 4 ? result.Message[3] : string.Empty;

		return report;
	}
}