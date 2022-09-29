using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class AddPercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return null;
        var percent = (double)value;
        return percent + "%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}