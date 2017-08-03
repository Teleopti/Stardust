using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Anywhere
{
	public class AnywhereAreaRegistration : AreaRegistration
	{
		public override string AreaName => "Anywhere";

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"Anywhere-authentication",
				"Anywhere/Authentication/{action}",
				new {controller = "Authentication", action = "SignIn", area = "Start", origin = "Anywhere"},
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"Anywhere-default",
				"Anywhere/{controller}/{action}",
				new {controller = "Application", action = "Redirect"}
			);
		}
	}
}