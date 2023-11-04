using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class ReverseVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            return (Visibility)value switch
            {
                > Visibility.Collapsed => Visibility.Visible,
                > Visibility.Visible => Visibility.Collapsed,
                _ => Visibility.Collapsed
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);Console.WriteLine(GetType());
            throw;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}