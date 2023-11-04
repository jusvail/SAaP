using System;
using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

[Table(Name = "RemindMessageData")]
public class RemindMessageData
{
    [Column]
    [PrimaryKey, Identity]
    public int RemindIndex { get; set; }

    [Column]
    public string Message { get; set; }

    [Column]
    public DateTime AddedDateTime { get; set; }

    [Column]
    public DateTime ModifiedDateTime { get; set; }
}