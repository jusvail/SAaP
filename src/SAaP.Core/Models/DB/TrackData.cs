using LinqToDB.Mapping;

namespace SAaP.Core.Models.DB;

public enum TrackType
{
    Filter = 0,
    Monitor = 1,
    Unknown = 2
}

[Table(Name = "TrackData")]
public class TrackData
{
    [Column]
    [PrimaryKey, Identity]
    public int TrackIndex { get; set; }

    [Column]
    [PrimaryKey]
    public string TrackName { get; set; }

    [Column]
    public string TrackContent { get; set; }

    [Column]
    public string TrackSummary { get; set; }

    [Column]
    public TrackType TrackType { get; set; }

}