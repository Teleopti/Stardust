using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(15)]
	public class RtaStartupTask : IBootstrapperTask
	{
		private readonly IAdherenceAggregatorInitializor _initializor;

		public RtaStartupTask(IAdherenceAggregatorInitializor initializor)
		{
			_initializor = initializor;
		}

		public Task Execute(IAppBuilder application)
		{
			_initializor.Initialize();
			return null;
		}
	}
}