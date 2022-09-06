using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "Stock")]
public class Stock
{

    [PrimaryKey]
    public string CodeName { get; set; }

    [Column]
    public string CompanyName { get; set; }

    [Column]
    public int BelongTo { get; set; }

    [Column]
    public int GroupId { get; set; }
}