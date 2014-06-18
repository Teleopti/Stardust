using System.Threading.Tasks;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(2)]
	public class RtaStartupTask : IBootstrapperTask
	{
		private readonly IAdherenceAggregatorInitializor _initializor;

		public RtaStartupTask(IAdherenceAggregatorInitializor initializor)
		{
			_initializor = initializor;
		}

		public Task Execute()
		{
			_initializor.Initialize();
			return null;
		}
	}
}