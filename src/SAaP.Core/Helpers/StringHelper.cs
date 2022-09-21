using SAaP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SAaP.Core.Helpers;

public static class StringHelper
{
    private static IEnumerable<string> FormattingWithComma(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        var trimmed = Regex.Replace(input.Trim(), "'|\"|\r|\r\n|\n|,|，|“|”|‘|’", " ");

        return Regex.Split(trimmed, @"\s+");
    }

    /// <summary>
    /// split string by '|\"|\r|\r\n|\n|,|，|“|”|‘|’"
    /// return a trimmed string list
    /// </summary>
    /// <param name="codeInput">input</param>
    /// <returns>trimmed string list</returns>
    public static List<string> FormatInputCode(string codeInput)
    {
        // format input
        var codes = FormattingWithComma(codeInput);
        // check null input
        if (codes == null) return null;

        // check code accuracy
        // no need to cut tdx 7 length to  6 length right now
        var accuracyCodes = codes.ToList(); // StockService.CheckStockCodeAccuracy(codes).ToList();
        // check null code
        if (accuracyCodes.Count == 0) return null;

        // delete repeat code
        accuracyCodes = accuracyCodes.GroupBy(a => a).Select(s => s.First()).ToList();

        return accuracyCodes;
    }
}