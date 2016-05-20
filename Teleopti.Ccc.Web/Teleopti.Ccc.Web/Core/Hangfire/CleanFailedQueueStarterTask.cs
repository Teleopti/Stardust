using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[TaskPriority(200)]
	public class CleanFailedQueueStarterTask : IBootstrapperTask
	{
		private readonly CleanFailedQueuePublisher _cleanFailedQueuePublisher;
		public CleanFailedQueueStarterTask(CleanFailedQueuePublisher cleanFailedQueuePublisher)
		{
			_cleanFailedQueuePublisher = cleanFailedQueuePublisher;
		}

		public Task Execute(IAppBuilder application)
		{
			_cleanFailedQueuePublisher.Start();
			return null;
		}

	}
}