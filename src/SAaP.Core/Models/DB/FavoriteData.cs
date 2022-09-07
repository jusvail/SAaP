using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "FavoriteData")]
public class FavoriteData
{
    [Column]
    [PrimaryKey]
    public int Id { get; set; }

    [PrimaryKey]
    [Column]
    public string Code { get; set; }

    [Column]
    public string GroupName { get; set; }
}