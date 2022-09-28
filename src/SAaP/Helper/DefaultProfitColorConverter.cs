using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace SAaP.Helper;

public class DefaultProfitColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        
        if (value == null) return new SolidColorBrush(Colors.White);

        return (double)value switch
        {
            > 10.0 => new SolidColorBrush(Colors.DarkRed),
            > 0.0 => new SolidColorBrush(Colors.OrangeRed),
            > -10.0 => new SolidColorBrush(Colors.Green),
            _ => new SolidColorBrush(Colors.DarkGreen)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}