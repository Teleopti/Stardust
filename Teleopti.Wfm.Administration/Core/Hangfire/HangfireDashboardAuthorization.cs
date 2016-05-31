using System.Collections.Generic;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireDashboardAuthorization : IAuthorizationFilter
	{
		public bool Authorize(IDictionary<string, object> owinEnvironment)
		{
			var context = new OwinContext(owinEnvironment);
			return isAuthenticated(context) && isTenantAdmin(context);
		}

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
	}
}