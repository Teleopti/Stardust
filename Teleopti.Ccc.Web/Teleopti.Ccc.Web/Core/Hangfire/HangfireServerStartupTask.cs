using System;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[TaskPriority(100)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		private readonly HangfireServerStarter _starter;

		public HangfireServerStartupTask(HangfireServerStarter starter)
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