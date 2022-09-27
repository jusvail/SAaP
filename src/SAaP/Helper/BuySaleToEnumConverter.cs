using Microsoft.UI.Xaml.Data;
using SAaP.Core.Models.DB;

namespace SAaP.Helper;

internal class BuySaleToEnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var target = System.Convert.ToInt32(parameter);

        var from = (TradeType)value;

        return Equals(target, (int)from);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? (TradeType)System.Convert.ToInt32(parameter) : TradeType.Unknown;
    }
}