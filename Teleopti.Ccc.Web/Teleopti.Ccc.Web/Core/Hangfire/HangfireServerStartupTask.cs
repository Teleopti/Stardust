using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[UseOnToggle(Toggles.RTA_HangfireEventProcessing_31593)]
	[TaskPriority(100)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		private readonly IConfigReader _config;
		private readonly IHangfireServerStarter _starter;

		public HangfireServerStartupTask(IConfigReader config, IHangfireServerStarter starter)
		{
			_config = config;
			_starter = starter;
		}

		public Task Execute(IAppBuilder application)
		{
			_starter.Start(application, _config.ConnectionStrings["Hangfire"].ConnectionString);
			return null;
		}

	}
}