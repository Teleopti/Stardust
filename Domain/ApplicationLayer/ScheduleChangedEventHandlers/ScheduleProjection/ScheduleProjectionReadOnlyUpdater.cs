using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyUpdater :
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly ScheduleProjectionReadOnlyChecker _scheduleProjectionReadOnlyChecker;


		public ScheduleProjectionReadOnlyUpdater(ScheduleProjectionReadOnlyChecker scheduleProjectionReadOnlyChecker)
		{
			_scheduleProjectionReadOnlyChecker = scheduleProjectionReadOnlyChecker;
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			_scheduleProjectionReadOnlyChecker.Execute(@event);
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			_scheduleProjectionReadOnlyChecker.Execute(@event);
		}
	}
}
