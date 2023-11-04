using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using SAaP.Core.Models.Monitor;

namespace SAaP.Helper;

internal class BsColorFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return new SolidColorBrush(Colors.Gray);

        return App.GetEnum<DealDirection>(value.ToString()) switch
        {
            DealDirection.Buy => new SolidColorBrush(Colors.IndianRed),
            DealDirection.Sell => new SolidColorBrush(Colors.LightGreen),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}