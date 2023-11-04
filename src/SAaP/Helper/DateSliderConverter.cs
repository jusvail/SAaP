using Microsoft.UI.Xaml.Data;

namespace SAaP.Helper;

public class DateSliderConverter : IValueConverter
{
	public static List<DateTimeOffset> SliderRange = new();

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		//if (parameter is not List<DateTimeOffset> dl) return value;

		try
		{
			var tar = (DateTimeOffset)value;

			var index = 0;

			for (; index < SliderRange.Count - 1; index++)
			{
				var s = SliderRange[index];
				//var e = SliderRange[index + 1];
				if (tar > s)
				{
					break;
				}
			}

			if (index > 250)
			{
				index = 250;
			}
			else
			{
				index--;
			}

			//index = SliderRange.FindIndex(d => d.Year == tar.Year && d.Month == tar.Month && d.Day == tar.Day);

			return index >= 0 ? System.Convert.ToDouble(index) : 0d;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return null;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		//if (parameter is not List<DateTimeOffset> dl) return value;

		try
		{
			var tar = (int)(double)value;
			return SliderRange[tar];
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return null;
		}
	}
}