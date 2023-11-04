using SAaP.Analyst.Models;
using SAaP.Analyst.Services;
using SAaP.Core.Services.Generic;

namespace SAaP.Analyst.Pipe;

internal class Plumber
{
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
        computingData.LineData[LineForm.Cci] = PlumbersWife.CalculateCciLine(computingData.OriginalDatas);
    }

    public void IndicatePattern(ComputingData computingData)
    {
        var dataIndex = computingData.OriginalDatas.Count - 1;

        PatternIndicator patternIndicator = new NFoldPatternIndicator(computingData, dataIndex - 300 + 1, dataIndex);

        computingData.MatchResult = patternIndicator.Indicate();
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

        if (result.Step1Found) report.Step1 = result.Message[0];
        if (result.Step2Found) report.Step2 = result.Message[1];
        if (result.Step3Found) report.Step3 = result.Message[2];
        if (result.Step4Found) report.Step4 = result.Message[3];

        return report;
    }
}