using System.Drawing;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class ColorExtensions
	{
		public static string ToStyleClass(this Color color)
		{
			var colorCode = color.ToArgb();
			return string.Concat("color_", ColorTranslator.ToHtml(Color.FromArgb(colorCode)).Replace("#", ""));
		}

		public static string ToHtml(this Color color)
		{
			return ColorTranslator.ToHtml(color);
		}

	}
}