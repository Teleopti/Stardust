using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenanceStardustEventHandler :
		IHandleEvent<IndexMaintenanceStardustEvent>,
		IRunOnStardust
	{
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;

		public IndexMaintenanceStardustEventHandler(IIndexMaintenanceRepository indexMaintenanceRepository)
		{
			_indexMaintenanceRepository = indexMaintenanceRepository;
		}

		[TenantScope]
		public virtual void Handle(IndexMaintenanceStardustEvent @event)
		{
			_indexMaintenanceRepository.Run();
		}
	}
}