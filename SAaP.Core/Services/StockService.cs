using System.Collections.Generic;
using System.Text;

namespace SAaP.Core.Services
{
    public static class StockService
    {
        private const int StandardCodeLength = 6;
        private const int TdxCodeLength = 7;

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
                sb.Append(arg).Append(",");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
