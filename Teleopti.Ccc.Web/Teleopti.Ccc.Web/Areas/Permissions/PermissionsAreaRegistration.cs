using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Permissions
{
	public class PermissionsRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Permissions";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"Permissions-authentication",
				"Permissions/Authentication/{action}",
				new { controller = "Authentication", action = "SignIn", area = "Start", origin = "Permissions" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start";
		}
	}
}
