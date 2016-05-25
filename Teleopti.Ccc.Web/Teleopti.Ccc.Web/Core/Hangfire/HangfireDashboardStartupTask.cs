using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[TaskPriority(101)]
	public class HangfireDashboardStartupTask : IBootstrapperTask
	{
		private readonly IConfigReader _config;

		public HangfireDashboardStartupTask(IConfigReader config)
		{
			_config = config;
		}

		public Task Execute(IAppBuilder application)
		{
			if (_config.ReadValue("HangfireDashboard", true))
				application.UseHangfireDashboard("/Hangfire", new DashboardOptions { AuthorizationFilters = new IAuthorizationFilter[] { new HangfireDashboardAuthorization() } });
			return null;
		}
	}
}