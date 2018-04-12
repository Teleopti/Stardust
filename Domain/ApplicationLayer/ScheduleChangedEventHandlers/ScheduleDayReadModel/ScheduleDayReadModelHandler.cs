using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelHandlerHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
		IRunOnHangfire
	{
		private readonly ScheduleDayReadModelPersister _scheduleDayReadModelPersister;

		public ScheduleDayReadModelHandlerHangfire(ScheduleDayReadModelPersister scheduleDayReadModelPersister)
		{
			_scheduleDayReadModelPersister = scheduleDayReadModelPersister;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			_scheduleDayReadModelPersister.Execute(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			_scheduleDayReadModelPersister.Execute(@event);
		}
	}
}