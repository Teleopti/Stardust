using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MobileReports
{
	public class MobileReportsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "MobileReports"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{

			var mapRoute = context.MapRoute(
				"MobileReports-authentication-signin",
				"MobileReports/Authentication/SignIn",
				new { controller = "Authentication", action = "MobileSignIn", area = "Start", origin= "MobileReports" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			mapRoute = context.MapRoute(
				"MobileReports-authentication",
				"MobileReports/Authentication/{action}",
				new { controller = "Authentication", action = "Index", area = "Start", origin = "MobileReports" },
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"MobileReports-default",
				"MobileReports/{controller}/{action}",
				new {controller = "Report", action = "Index"}
				);
		}
	}
}