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
			context.MapRoute(
				"HealthCheck_default",
				"HealthCheck/{controller}/{action}/{id}",
				new { controller = "Application", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
