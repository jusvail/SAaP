using SAaP.Core.Models.DB;
using System;

namespace SAaP.Core.Services.Generic;

public static class CalculationService
{
    /// <summary>
    /// keep 2 decimal point
    /// </summary>
    /// <param name="input">input</param>
    /// <returns>result</returns>
    public static double Round2(double input)
    {
        return double.IsNaN(input) ? 0.0 : Math.Round(input, 2);
    }

    public static double Round3(double input)
    {
        return double.IsNaN(input) ? 0.0 : Math.Round(input, 3);
    }

    public static double CalcTtm(double before, double after) =>
        Round2(100 * (after - before) / before);

    /// <summary>
    /// don't reverse yesterday and today!!!
    /// </summary>
    /// <param name="yesterday">OriginalData of yesterday</param>
    /// <param name="today">OriginalData of today</param>
    /// <returns></returns>
    public static double CalcOverprice(OriginalData yesterday, OriginalData today)
        => CalcTtm(yesterday.Ending, today.High);

    public static double TryParseStringToDouble(string input)
    {
        return double.TryParse(input, out var result) ? result : 0;
    }

    public static int TryParseStringToInt(string input)
    {
        return int.TryParse(input, out var result) ? result : 0;
    }
}