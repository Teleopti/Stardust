using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenanceStardustHandler :
		IHandleEvent<IndexMaintenanceStardust>,
		IRunOnStardust
	{
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;

		public IndexMaintenanceStardustHandler(IIndexMaintenanceRepository indexMaintenanceRepository)
		{
			_indexMaintenanceRepository = indexMaintenanceRepository;
		}

		public virtual void Handle(IndexMaintenanceStardust @event)
		{
			_indexMaintenanceRepository.Run();
			
		}
	}
}