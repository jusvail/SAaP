using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class StatisticsResult
{
	public OriginalData BuyDate         { get; set; }
	public OriginalData SellDate        { get; set; }
	public OriginalData MaxPullBackDate { get; set; }
	public Stock        Stock           { get; set; }

	public double PullUpBefore           { get; set; }
	public bool   IsSuccess              { get; set; }
	public double MaxPullBack            { get; set; }
	public int    DaysOfBuyToMaxPullBack { get; set; }
	public double Profit                 { get; set; }
	public int    DaysOfBuyToSell        { get; set; }
}