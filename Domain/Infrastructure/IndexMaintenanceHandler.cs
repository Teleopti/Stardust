using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	[EnabledBy(Toggles.ETL_FasterIndexMaintenance_38847)]
	public class IndexMaintenanceHandler :
		IHandleEvent<IndexMaintenanceEvent>,
		IRunOnHangfire
	{
		private readonly IEventPublisher _eventPublisher;

		public IndexMaintenanceHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		[RecurringJob]
		public virtual void Handle(IndexMaintenanceEvent @event)
		{
			_eventPublisher.Publish(new IndexMaintenanceStardustEvent
			{
				JobName = "Index Maintenance",
				UserName = "Index Maintenance"
			});
		}
	}
}