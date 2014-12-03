using System.Threading.Tasks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[UseOnToggle(Toggles.RTA_HangfireEventProcessing_31593)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		public Task Execute()
		{
			return null;
		}
	}
}