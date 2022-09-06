using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "ActivityData")]
public class ActivityData
{

    [PrimaryKey]
    public DateTime Date { get; set; }

    [Column]
    public string QueryString { get; set; }

    [Column]
    public string AnalyzeData { get; set; }
}