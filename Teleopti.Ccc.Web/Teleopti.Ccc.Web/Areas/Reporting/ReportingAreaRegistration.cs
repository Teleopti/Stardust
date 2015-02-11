using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Reporting
{
	public class ReportingAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Reporting";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
			 "Reporting-authentication",
			 "Reporting/Authentication/{action}",
			 new { controller = "Authentication", action = "SignIn", area = "Start", origin = "Reporting" },
			 null,
			 new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
			 "Reporting_default",
			  "Reporting/{controller}/{id}",
			  new { Controller = "Report", action = "Index", id = UrlParameter.Optional }

			);

			context.MapRoute(
			 "Reporting_index",
			  "Reporting/{controller}/{action}/{id}",
			  new { Controller = "Report", action = "Index", id = UrlParameter.Optional }

			);


		}
	}
}