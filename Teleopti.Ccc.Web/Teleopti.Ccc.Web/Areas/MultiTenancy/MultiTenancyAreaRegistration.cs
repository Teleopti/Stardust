using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class MultiTenancyAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "MultiTenancy";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var mapRoute = context.MapRoute(
				"MultiTenancy_Wfm",
				"Wfm/MultiTenancy/TenantAdminInfo/{action}",
				new { controller = "TenantAdminInfo", action = "Index", area = "MultiTenancy", origin = "WFM" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.MultiTenancy.*" });

			mapRoute.DataTokens["area"] = "MultiTenancy";

			context.MapRoute(
				"MultiTenancy",
				"MultiTenancy/TenantAdminInfo/{action}",
				new { controller = "TenantAdminInfo", action = "Index", area = "MultiTenancy" },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.MultiTenancy.*" });

		}
	}
}