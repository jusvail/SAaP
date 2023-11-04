using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper
{
    internal class RandomizedFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ColorHelper.GetRandomColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
