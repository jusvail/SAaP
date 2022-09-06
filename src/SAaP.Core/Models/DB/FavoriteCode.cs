using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "FavoriteCode")]
public class FavoriteCode
{
    public int Id { get; set; }

    public string GroupName { get; set; }

    public string Code { get; set; }
}