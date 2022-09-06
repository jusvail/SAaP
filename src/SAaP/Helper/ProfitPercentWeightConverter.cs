using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class ProfitPercentWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!double.TryParse(value.ToString(), out var profit))
        {
            return FontWeights.Normal;
        }

        return profit switch
        {
            > 5 => FontWeights.Bold,
            > 2 => FontWeights.Medium,
            > 0 => FontWeights.Normal,
            _ => FontWeights.Thin
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}