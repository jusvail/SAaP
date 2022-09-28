using Microsoft.UI.Xaml.Data;
using SAaP.Core.Services;

namespace SAaP.Helper;

internal class CodeNameFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string val) return null;

        switch (val.Length)
        {
            case StockService.TdxCodeLength:
                var success = int.TryParse(val[..1], out var belong);
                if (success)
                {
                    switch (belong)
                    {
                        case StockService.ShFlag:
                            return StockService.Sh + val.Substring(1, val.Length - 1);
                        case StockService.SzFlag:
                            return StockService.Sz + val.Substring(1, val.Length - 1);
                    }
                }
                break;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}