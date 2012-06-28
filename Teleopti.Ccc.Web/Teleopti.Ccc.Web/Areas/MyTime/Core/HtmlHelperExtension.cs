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
			return new ScheduleHtmlHelper();
		}

		public static PortalHtmlHelper Portal(this HtmlHelper htmlHelper)
		{
			return new PortalHtmlHelper();
		}

		public static LayoutBaseHtmlHelper LayoutBase(this HtmlHelper htmlHelper)
		{
			return new LayoutBaseHtmlHelper(htmlHelper);
		}

		public static string HtmlStyleLeft(this HtmlHelper htmlHelper)
		{
			return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "right" : "left";
		}
	}
}