using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using static System.Double;

namespace SAaP.Helper;

public class ProfitColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!TryParse(value.ToString(), out var profit))
        {
            return new SolidColorBrush(Colors.Black);
        }

        return profit switch
        {
            > 150 => new SolidColorBrush(Colors.DarkRed),
            > 100 => new SolidColorBrush(Colors.OrangeRed),
            > 80 => new SolidColorBrush(Colors.Green),
            _ => new SolidColorBrush(Colors.DarkGreen)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}