using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SAaP.Core.Helpers
{
    public static class StringHelper
    {
        public static IEnumerable<string> FormattingWithComma(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            var trimmed = Regex.Replace(input.Trim(), "'|\"|\r|\r\n|\n|,", " ");

            return Regex.Split(trimmed, @"\s+");
        }
    }
}
