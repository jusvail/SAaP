using SAaP.Analyst.Models;
using SAaP.Analyst.Pipe;

namespace SAaP.Analyst;

public class Analyst
{
    private readonly Plumber _plumber = new();

    public Analyst(RawData rawData)
    {
        ComputingData = new ComputingData(rawData);
    }

    private ComputingData ComputingData { get; }

    public Report Target()
    {
        // won't process if data count less than 150d
        if (ComputingData.HistoricDataCount < 150)
            return Report.ErrorWith(ComputingData.Stock.CodeNameFull, ComputingData.Stock.CompanyName, "数据<150条");

        Report report;

        try
        {
            // preprocess. generate all kind of line information
            _plumber.PreProcess(ComputingData);

            // loop by date. indicate all standard pattern for the historic k line
            _plumber.IndicatePattern(ComputingData);

            // generate report. contains possible pattern
            report = _plumber.GenerateReport(ComputingData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Report.ErrorWith(ComputingData.Stock.CodeNameFull, ComputingData.Stock.CompanyName, "程序错误");
        }

        return report;
    }
}