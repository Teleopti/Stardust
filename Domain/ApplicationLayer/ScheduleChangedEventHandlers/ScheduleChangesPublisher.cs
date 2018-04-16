using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangesPublisherHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly ScheduleChangesSubscriptionPublisher _scheduleChangesSubscriptionPublisher;

		public ScheduleChangesPublisherHangfire(ScheduleChangesSubscriptionPublisher scheduleChangesSubscriptionPublisher)
		{
			_scheduleChangesSubscriptionPublisher = scheduleChangesSubscriptionPublisher;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			_scheduleChangesSubscriptionPublisher.Send(@event);
		}
	}
}