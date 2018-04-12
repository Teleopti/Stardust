using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[EnabledBy(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleReadModelWrapperHandler :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
		IRunOnHangfire,
		IScheduleDayReadModelHandlerHangfire,
		IPersonScheduleDayReadModelUpdaterHangfire
	{
		private readonly ScheduleDayReadModelPersister _scheduleDayReadModelPersister;
		private readonly PersonScheduleDayReadModelUpdaterPersister _personScheduleDayReadModelUpdaterPersister;

		public ScheduleReadModelWrapperHandler(ScheduleDayReadModelPersister scheduleDayReadModelPersister,
			PersonScheduleDayReadModelUpdaterPersister personScheduleDayReadModelUpdaterPersister)
		{
			_scheduleDayReadModelPersister = scheduleDayReadModelPersister;
			_personScheduleDayReadModelUpdaterPersister = personScheduleDayReadModelUpdaterPersister;
		}

		[UnitOfWork]
		public void Handle(ProjectionChangedEvent @event)
		{
			_scheduleDayReadModelPersister?.Execute(@event);
			_personScheduleDayReadModelUpdaterPersister?.Execute(@event);
		}

		[UnitOfWork]
		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			_personScheduleDayReadModelUpdaterPersister.Execute(@event);
		}

		[UnitOfWork]
		public void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			_scheduleDayReadModelPersister.Execute(@event);
		}
	}
}