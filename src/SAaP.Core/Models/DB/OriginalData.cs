using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "OriginalData")]
public class OriginalData
{
	[PrimaryKey]
	public string CodeName { get; set; }

	[PrimaryKey]
	public int BelongTo { get; set; }

	[PrimaryKey]
	public string Day { get; set; }

	[Column]
	public int Volume { get; set; }

	[Column]
	public double Amount { get; set; }

	[Column]
	public double Opening { get; set; }

	[Column]
	public double High { get; set; }

	[Column]
	public double Low { get; set; }

	[Column]
	public double Ending { get; set; }
    //
    // [Column]
    // public double McInCirculation { get; set; }
    //
    // [Column]
    // public double Mc { get; set; }
    //
    // [Column]
    // public double Swap { get; set; }

}