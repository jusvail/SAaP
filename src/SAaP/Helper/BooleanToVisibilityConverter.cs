using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);Console.WriteLine(GetType());
            throw;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}