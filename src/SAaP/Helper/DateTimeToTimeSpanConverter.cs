using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class DateTimeToTimeSpanConverter : IValueConverter
{
    private const string DefaultStartTimeSpan = "9:30";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return TimeSpan.Parse(value == null ? DefaultStartTimeSpan : value.ToString()!);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            null => DefaultStartTimeSpan,
            TimeSpan timeSpan => $"{timeSpan.Hours}:{timeSpan.Minutes}",
            _ => DefaultStartTimeSpan
        };
    }
}