using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[DisabledBy(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleProjectionReadOnlyUpdater :
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire,
		IScheduleProjectionReadOnlyUpdater
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

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public interface IScheduleProjectionReadOnlyUpdater
	{
		void Handle(ProjectionChangedEvent @event);
		void Handle(ProjectionChangedEventForScheduleProjection @event);
	}
}
