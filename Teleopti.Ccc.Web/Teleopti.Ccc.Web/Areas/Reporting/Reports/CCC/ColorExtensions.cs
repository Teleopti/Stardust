using System.Drawing;

namespace Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC
{
	public static class ColorExtensions
	{
		public static bool IsDark(this Color color)
		{
			// source: http://stackoverflow.com/questions/946544/good-text-foreground-color-for-a-given-background-color
			var brightness = color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
			// 100 works better with green
			return brightness < 100;
		}
	}
}