using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SAaP.Core.Helpers;

public static class StringHelper
{
    public static IEnumerable<string> FormattingWithComma(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        var trimmed = Regex.Replace(input.Trim(), "'|\"|\r|\r\n|\n|,|，|“|”|‘|’", " ");

        return Regex.Split(trimmed, @"\s+");
    }
}