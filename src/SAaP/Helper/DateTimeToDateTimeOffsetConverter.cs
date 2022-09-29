using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is not DateTime time ? DateTimeOffset.Now : time.ToUniversalTime();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not DateTimeOffset time ? DateTime.Now : time.Date;
    }
}