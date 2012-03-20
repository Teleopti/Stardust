using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class HtmlHelperExtension
	{
		public static ScheduleHtmlHelper Schedule(this HtmlHelper htmlHelper)
		{
			return new ScheduleHtmlHelper(htmlHelper);
		}

		public static PortalHtmlHelper Portal(this HtmlHelper htmlHelper)
		{
			return new PortalHtmlHelper(htmlHelper);
		}

		public static LayoutBaseHtmlHelper LayoutBase(this HtmlHelper htmlHelper)
		{
			return new LayoutBaseHtmlHelper(htmlHelper);
		}

		public static string HtmlStyleLeft(this HtmlHelper htmlHelper)
		{
			return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "right" : "left";
		}

		//public static string MixColor(this HtmlHelper htmlHelper, string colorString, int add)
		//{
		//    var red = int.Parse(colorString.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
		//    var green = int.Parse(colorString.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
		//    var blue = int.Parse(colorString.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
		//    red += add;
		//    green += add;
		//    blue += add;
		//    if (red > 255) red = 255;
		//    if (green > 255) green = 255;
		//    if (blue > 255) blue = 255;
		//    if (red < 0) red = 0;
		//    if (green < 0) green = 255;
		//    if (blue < 0) blue = 255;
		//    var color = Color.FromArgb(red, green, blue);
		//    return color.ToHtml();
		//}
	}
}