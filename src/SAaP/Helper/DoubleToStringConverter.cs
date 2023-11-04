using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

internal class DoubleToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		try
		{
			return value?.ToString();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		try
		{
			var val = value.ToString();
			val = Regex.Replace(val!, "[^\\d]", "");

			return string.IsNullOrEmpty(val) ? 0d : double.Parse(val);
		}
		catch (Exception)
		{
			return 0d;
		}
	}
}