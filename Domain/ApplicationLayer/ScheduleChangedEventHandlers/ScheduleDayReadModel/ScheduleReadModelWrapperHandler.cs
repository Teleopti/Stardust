using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[EnabledBy(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleReadModelWrapperHandler :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForScheduleDay>,
		IRunOnHangfire,
		IScheduleDayReadModelHandlerHangfire
	{
		private readonly ScheduleDayReadModelHandlerHangfire _scheduleDayReadModelHandlerHangfire;

		public ScheduleReadModelWrapperHandler(ScheduleDayReadModelHandlerHangfire scheduleDayReadModelHandlerHangfire)
		{
			_scheduleDayReadModelHandlerHangfire = scheduleDayReadModelHandlerHangfire;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			_scheduleDayReadModelHandlerHangfire.Handle(@event);
		}

		public void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			_scheduleDayReadModelHandlerHangfire.Handle(@event);
		}
	}
}