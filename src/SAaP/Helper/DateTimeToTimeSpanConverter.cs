using Microsoft.UI.Xaml.Data;
using SAaP.Constant;

namespace SAaP.Helper;

internal class DateTimeToTimeSpanConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return TimeSpan.Parse(value == null ? PjConstant.DefaultStartTimeSpan : value.ToString()!);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            null => PjConstant.DefaultStartTimeSpan,
            TimeSpan timeSpan => $"{timeSpan.Hours}:{timeSpan.Minutes}",
            _ => PjConstant.DefaultStartTimeSpan
        };
    }
}