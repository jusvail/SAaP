using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class ProfitWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!double.TryParse(value.ToString(), out var profit))
        {
            return FontWeights.Normal;
        }

        return profit switch
        {
            > 150 => FontWeights.Bold,
            > 100 => FontWeights.Medium,
            > 80 => FontWeights.Normal,
            _ => FontWeights.Thin
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}