using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[UseOnToggle(Toggles.Wfm_Use_Stardust)]
	[TaskPriority(100)]
	public class StardustStartupTask : IBootstrapperTask
	{
		private readonly StardustServerStarter _starter;

		public StardustStartupTask(StardustServerStarter starter)
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