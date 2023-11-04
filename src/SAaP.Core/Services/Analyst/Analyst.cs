using System;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Services.Analyst.Pipe;

namespace SAaP.Core.Services.Analyst;

public class Analyst
{
	private readonly Plumber _plumber = new(PatternType.NFoldPattern);

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
			Console.WriteLine(e);Console.WriteLine(GetType());
			return Report.ErrorWith(ComputingData.Stock.CodeNameFull, ComputingData.Stock.CompanyName, "程序错误");
		}

		return report;
	}
}