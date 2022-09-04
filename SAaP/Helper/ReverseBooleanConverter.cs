using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class ReverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            return !(bool)value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}