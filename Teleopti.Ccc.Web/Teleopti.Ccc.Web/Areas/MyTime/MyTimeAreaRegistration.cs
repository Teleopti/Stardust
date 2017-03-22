using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MyTime
{
	public class MyTimeAreaRegistration : AreaRegistration
	{
		public override string AreaName => "MyTime";

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"MyTime-authentication",
				"MyTime/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "MyTime" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"MyTime-widget-alias",
				"MyTime/ASMWidget/{action}",
				new { controller = "Widget", action = "Index" }
				);

			context.MapRoute(
				"MyTime-calendar",
				"MyTime/Share",
				new { controller = "ShareCalendar", action = "iCal" }
				);

			context.MapRoute(
				"MyTime-date-route",
				"MyTime/{controller}/{action}/{year}/{month}/{day}",
				new {},
				new {year = @"\d{4}", month = @"\d{2}", day = @"\d{2}"}
				);

			context.MapRoute(
				"MyTime-date-id-route",
				"MyTime/{controller}/{action}/{year}/{month}/{day}/{id}",
				new {id = UrlParameter.Optional},
				new {year = @"\d{4}", month = @"\d{2}", day = @"\d{2}", id = new GuidConstraint()}
				);

			context.MapRoute(
				"MyTime-default",
				"MyTime/{controller}/{action}/{id}",
				new { controller = "Portal", action = "Index", id = UrlParameter.Optional }
				);

		}
	}
}
