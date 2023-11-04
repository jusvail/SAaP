using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using static System.Double;

namespace SAaP.Helper;

public class ProfitPercentColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!TryParse(value.ToString(), out var profit))
        {
            return new SolidColorBrush(Colors.Black);
        }

        return profit switch
        {
            > 5.0 => new SolidColorBrush(Colors.DarkRed),
            > 2.0 => new SolidColorBrush(Colors.OrangeRed),
            > 0.0 => new SolidColorBrush(Colors.Green),
            _ => new SolidColorBrush(Colors.DarkGreen)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}