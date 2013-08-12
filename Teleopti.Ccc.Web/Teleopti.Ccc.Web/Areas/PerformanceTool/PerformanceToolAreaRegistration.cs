using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool
{
	public class PerformanceToolAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "PerformanceTool"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"PerformanceTool-authentication",
				"PerformanceTool/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "PerformanceTool" },
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"PerformanceTool-default",
				"PerformanceTool/{controller}/{action}",
				new { controller = "Application", action = "Index" }
				);
		}
	}

}