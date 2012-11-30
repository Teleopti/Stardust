using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Team
{
	public class TeamAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "Team"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"Team-authentication",
				"Team/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "Team" },
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			context.MapRoute(
				"Team-default",
				"Team/{controller}/{action}",
				new { controller = "Application", action = "Index" }
				);
		}
	}

}