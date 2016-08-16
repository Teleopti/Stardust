using System.Collections.Generic;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireDashboardAuthorization : IDashboardAuthorizationFilter
	{
		private static bool isTenantAdmin(IOwinContext context)
		{
			if (context.Authentication.User == null)
				return false;

			return context.Authentication.User.IsInRole(HangfireCookie.TenantAdminRoleName);
		}

		private static bool isAuthenticated(IOwinContext context)
		{
			return context.Authentication.User.Identity.IsAuthenticated;
		}

		public bool Authorize(DashboardContext context)
		{
			var owinContext = new OwinContext(context.GetOwinEnvironment());
			return isAuthenticated(owinContext) && isTenantAdmin(owinContext);
		}
	}
}