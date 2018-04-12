using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[EnabledBy(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleReadModelWrapperHandler :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
		IRunOnHangfire,
		IScheduleDayReadModelHandlerHangfire,
		IPersonScheduleDayReadModelUpdaterHangfire,
		IScheduleProjectionReadOnlyUpdater
	{
		private readonly ScheduleDayReadModelPersister _scheduleDayReadModelPersister;
		private readonly PersonScheduleDayReadModelUpdaterPersister _personScheduleDayReadModelUpdaterPersister;
		private readonly ScheduleProjectionReadOnlyChecker _scheduleProjectionReadOnlyChecker;

		public ScheduleReadModelWrapperHandler(ScheduleDayReadModelPersister scheduleDayReadModelPersister,
			PersonScheduleDayReadModelUpdaterPersister personScheduleDayReadModelUpdaterPersister,
			ScheduleProjectionReadOnlyChecker scheduleProjectionReadOnlyChecker)
		{
			_scheduleDayReadModelPersister = scheduleDayReadModelPersister;
			_personScheduleDayReadModelUpdaterPersister = personScheduleDayReadModelUpdaterPersister;
			_scheduleProjectionReadOnlyChecker = scheduleProjectionReadOnlyChecker;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			_scheduleDayReadModelPersister?.Execute(@event);
			_personScheduleDayReadModelUpdaterPersister?.Execute(@event);
			_scheduleProjectionReadOnlyChecker?.Execute(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			_scheduleProjectionReadOnlyChecker.Execute(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			_personScheduleDayReadModelUpdaterPersister.Execute(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			_scheduleDayReadModelPersister.Execute(@event);
		}
	}
}