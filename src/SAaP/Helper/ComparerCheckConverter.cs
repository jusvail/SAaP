using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class ComparerCheckConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var target = (string)parameter;

        var from = (string)value;

        return Equals(target, from);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? parameter : null;
    }
}