using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class WfmAreaRegistration : AreaRegistration
	{

		public override string AreaName
		{
            get { return "WFM"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"WFM-authentication",
				"WFM/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "WFM" },
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"WFM-default",
				"WFM");
		}

	}
}