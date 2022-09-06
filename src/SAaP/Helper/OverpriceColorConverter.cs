using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using Microsoft.UI.Text;

namespace SAaP.Helper;

internal class OverpriceColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!double.TryParse(value.ToString(), out var overprice))
        {
            throw new NotImplementedException();
        }

        if (targetType == typeof(FontWeight))
        {
            return overprice switch
            {
                > 100.0 => FontWeights.Bold,
                > 90.0 => FontWeights.Medium,
                > 80.0 => FontWeights.Normal,
                > 50.0 => FontWeights.Normal,
                _ => FontWeights.Thin
            };
        }
        if (targetType == typeof(Brush))
        {
            return overprice switch
            {
                >= 100.0 => new SolidColorBrush(Colors.DarkRed),
                >= 90.0 => new SolidColorBrush(Colors.OrangeRed),
                >= 80.0 => new SolidColorBrush(Colors.Black),
                >= 50.0 => new SolidColorBrush(Colors.Green),
                _ => new SolidColorBrush(Colors.DarkGreen)
            };
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}