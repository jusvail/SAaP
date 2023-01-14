using System;

namespace SAaP.Core.Models;

public class MinuteData
{
    public string CodeName { get; set; }

    public int BelongTo { get; set; }

    public DateTime FullTime { get; set; }

    public int Volume { get; set; }

    public double Opening { get; set; }

    public double High { get; set; }

    public double Low { get; set; }

    public double Ending { get; set; }

}