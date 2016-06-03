using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenanceHandler :
		IHandleEvent<IndexMaintenance>,
		IRunOnHangfire
	{
		private readonly IEventPublisher _eventPublisher;

		public IndexMaintenanceHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		[RecurringId("IndexMaintenanceHandler:::IndexMaintenance")]
		public virtual void Handle(IndexMaintenance @event)
		{
			_eventPublisher.Publish(new IndexMaintenanceStardust());
		}
	}
}