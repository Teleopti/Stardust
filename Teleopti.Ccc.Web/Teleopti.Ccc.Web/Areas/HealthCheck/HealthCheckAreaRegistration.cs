using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.HealthCheck
{
	public class HealthCheckAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "HealthCheck";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"HealthCheck-authentication",
				"HealthCheck/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "HealthCheck" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"HealthCheck_default",
				"HealthCheck/{controller}/{action}",
				new {controller = "Application", action = "Index"}
				);
		}
	}
}
