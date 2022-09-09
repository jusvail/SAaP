using Microsoft.UI.Xaml.Data;
using SAaP.Core.Helpers;

namespace SAaP.Helper;

internal class SimplifySearchStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var searchString = value as string;

        if (string.IsNullOrEmpty(searchString)) return value;

        var codes = StringHelper.FormatInputCode(searchString);

        return !codes.Any() ? value : $"[{codes[0]}] 等{codes.Count}个代码";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}