using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class AddPercentConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value == null)
			return "0%";
		return value + "%";
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}