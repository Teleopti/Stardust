using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	[EnabledBy(Toggles.ETL_FasterIndexMaintenance_38847)]
	public class IndexMaintenanceHangfireEventHandler :
		IHandleEvent<IndexMaintenanceHangfireEvent>,
		IRunOnHangfire
	{
		private readonly IEventPublisher _eventPublisher;

		public IndexMaintenanceHangfireEventHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		[RecurringId("IndexMaintenanceHangfireEventHandler:::IndexMaintenanceHangfireEvent")]
		public virtual void Handle(IndexMaintenanceHangfireEvent @event)
		{
			_eventPublisher.Publish(new IndexMaintenanceStardustEvent
			{
				JobName = "Index Maintenance",
				UserName = "Index Maintenance"
			});
		}
	}
}