using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "FavoriteData")]
public class FavoriteData
{

    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column]
    public string GroupName { get; set; }

    [Column]
    public string Code { get; set; }
}