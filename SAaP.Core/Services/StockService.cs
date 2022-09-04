using System.Collections.Generic;
using System.Text;

namespace SAaP.Core.Services
{
    public static class StockService
    {
        private const int StandardCodeLength = 6;
        private const int TdxCodeLength = 7;

        private const string ShCsvName = @"sh{0}.csv";
        private const string SzCsvName = @"sz{0}.csv";

        public const string Sh = "sh";
        public const string Sz = "sz";

        public static string GetOutputNameSh(string codeName) => string.Format(ShCsvName, codeName);
        public static string GetOutputNameSz(string codeName) => string.Format(SzCsvName, codeName);

        public static IEnumerable<string> CheckStockCodeAccuracy(IEnumerable<string> inputs)
        {
            foreach (var input in inputs)
            {
                switch (input.Length)
                {
                    case StandardCodeLength:
                        yield return input;
                        break;
                    case TdxCodeLength:
                        yield return input[1..];
                        break;
                }
            }
        }

        public static string FormatPyArgument(IEnumerable<string> args)
        {
            var sb = new StringBuilder();

            foreach (var arg in args)
            {
                sb.Append(arg).Append(',');
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
