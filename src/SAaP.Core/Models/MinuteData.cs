using System;

namespace SAaP.Core.Models;

public class MinuteData
{
    public string CodeName { get; set; }

    public string CompanyName { get; set; }

    public int BelongTo { get; set; }

    public DateTimeOffset FullTime { get; set; }

    public int Volume { get; set; }

    public double Opening { get; set; }

    public double High { get; set; }

    public double Low { get; set; }

    public double Ending { get; set; }

    public double Zd()
    {
        return 100 * (Ending - Opening) / Opening;
    }

    public double Money()
    {
        return 10 * Volume * Ending;
    }

}