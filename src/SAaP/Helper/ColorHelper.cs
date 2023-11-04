
using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace SAaP.Helper;

internal class ColorHelper
{
	public static SolidColorBrush GetRandomColor()
	{
		var random = new Random();

		var solidColorBrush = new SolidColorBrush(Color.FromArgb((byte)random.Next(50, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255)));


		return solidColorBrush;
	}
}