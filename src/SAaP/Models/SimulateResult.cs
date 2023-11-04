using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class SimulateResult
{
	public OriginalData BuyDate    { get; set; }
	public OriginalData D30SellDate   { get; set; }
	public int          D30PassingDay { get; set; }
	public bool         D30Success    { get; set; }
	public double       D30Pullback   { get; set; }
	public int          D60PassingDay { get; set; }
	public bool         D60Success    { get; set; }
	public double       D60Pullback   { get; set; }
	public double       D30Profit     { get; set; }
	public double       D60Profit     { get; set; }
	public string       CodeName      { get; set; }
	public string       CompanyName   { get; set; }
	public OriginalData D60SellDate   { get; set; }
}