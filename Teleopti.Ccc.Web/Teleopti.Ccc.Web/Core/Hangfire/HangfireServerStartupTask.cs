using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[UseOnToggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[TaskPriority(100)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		private readonly IHangfireServerStarter _starter;

		public HangfireServerStartupTask(IHangfireServerStarter starter)
		{
			_starter = starter;
		}

		public Task Execute(IAppBuilder application)
		{
			_starter.Start(application);
			return null;
		}

	}
}