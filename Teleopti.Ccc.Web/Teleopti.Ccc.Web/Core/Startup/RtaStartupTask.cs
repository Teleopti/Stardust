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
		private readonly IRta _initializor;
		private readonly IStateStreamSynchronizer _synchronizer;

		public RtaStartupTask(IRta initializor, IStateStreamSynchronizer synchronizer)
		{
			_initializor = initializor;
			_synchronizer = synchronizer;
		}

		public Task Execute(IAppBuilder application)
		{
			//todo: tenant how to solve this?? foreach datasource????
			_initializor.Initialize("Teleopti WFM");
			_synchronizer.Initialize("Teleopti WFM");
			return null;
		}
	}
}