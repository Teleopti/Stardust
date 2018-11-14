using System;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Owin;
using Microsoft.Owin;
using Owin;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireDashboardStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;

		public HangfireDashboardStarter(
			HangfireStarter starter,
			IConfigReader config)
		{
			_starter = starter;
			_config = config;
		}

		public void Start(IAppBuilder app, Func<IOwinDashboardAntiforgery> antiForgery)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));

			if (_config.ReadValue("HangfireDashboard", true))
				app.UseHangfireDashboard("/hangfire", new DashboardOptions
				{
					Authorization = new[] {new HangfireDashboardAuthorization()},
					AppPath = null,
				}, JobStorage.Current, antiForgery());
		}
	}

	public class HangfireDashboardAuthorization : IDashboardAuthorizationFilter
	{
		public static string TenantAdminRoleName = "TenantAdmin";

		private static bool isTenantAdmin(IOwinContext context)
		{
			if (context.Authentication.User == null)
				return false;

			return context.Authentication.User.IsInRole(TenantAdminRoleName);
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