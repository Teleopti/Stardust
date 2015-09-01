using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(15)]
	public class RtaStartupTask : IBootstrapperTask
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _initializor;
		private readonly IStateStreamSynchronizer _synchronizer;

		public RtaStartupTask(Domain.ApplicationLayer.Rta.Service.Rta initializor, IStateStreamSynchronizer synchronizer)
		{
			_initializor = initializor;
			_synchronizer = synchronizer;
		}

		public Task Execute(IAppBuilder application)
		{
			_initializor.Initialize();
			_synchronizer.Initialize();
			return null;
		}
	}
}