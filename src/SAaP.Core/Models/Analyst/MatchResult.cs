using System.Collections.Generic;

namespace SAaP.Core.Models.Analyst;

public class MatchResult
{
    public static readonly MatchResult Empty = new();

    //public List<OriginalData> KeyData { get; set; } = new();
    public List<string> Message { get; set; } = new();

    public bool Step1Found { get; set; }
    public bool Step2Found { get; set; }
    public bool Step3Found { get; set; }
    public bool Step4Found { get; set; }

    public bool Bought { get; set; }
    public string BoughtDay { get; set; } = string.Empty;
    public double BuyPrice { get; set; }
    public double BuyProgress { get; set; }
    public bool Sold { get; set; }
    public string SoldDay { get; set; } = string.Empty;
    public double SellPrice { get; set; }
    public int HoldingDays { get; set; }
    public string Profit { get; set; } = string.Empty;
    
    public bool Error { get; init; }
}