using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

public class DateTimeToRegularFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var dbDateTime = value is DateTime time ? time : default;

        return dbDateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}