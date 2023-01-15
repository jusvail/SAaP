using Microsoft.UI.Xaml.Data;
using SAaP.Core.Models.Monitor;

namespace SAaP.Helper;

internal class DirectionModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return "未知";

        if (int.TryParse(value.ToString(), out var mode))
        {

            return BuyMode.ModeDetails[mode];
        }

        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? parameter : null;
    }
}