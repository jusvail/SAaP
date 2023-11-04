using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class MoneyFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var money = (int?)value;

        return money > 0 ? money?.ToString("N0") : "未开盘/停牌";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}