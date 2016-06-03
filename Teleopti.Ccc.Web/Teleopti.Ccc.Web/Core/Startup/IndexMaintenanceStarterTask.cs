using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(200)]
	public class IndexMaintenanceStarterTask : IBootstrapperTask
	{
		private readonly IndexMaintenancePublisher _indexMaintenancePublisher;

		public IndexMaintenanceStarterTask(IndexMaintenancePublisher indexMaintenancePublisher)
		{
			_indexMaintenancePublisher = indexMaintenancePublisher;
		}

		public Task Execute(IAppBuilder application)
		{
			_indexMaintenancePublisher.Start();
			return null;
		}
	}
}