using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class ShiftEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ShiftEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info)
		{
			publishShiftStartEvent(info);
			publishShiftEndEvent(info);
		}

		private void publishShiftStartEvent(Context info)
		{
			if (!info.Schedule.ShiftStarted())
				return;
			_eventPublisher.Publish(new PersonShiftStartEvent
			{
				BelongsToDate = info.Schedule.BelongsToDate,
				PersonId = info.PersonId,
				ShiftStartTime = info.Schedule.CurrentShiftStartTime,
				ShiftEndTime = info.Schedule.CurrentShiftEndTime
			});
		}

		private void publishShiftEndEvent(Context info)
		{
			if (!info.Schedule.ShiftEnded())
				return;
			_eventPublisher.Publish(new PersonShiftEndEvent
			{
				BelongsToDate = info.Schedule.PreviousActivity().BelongsToDate,
				PersonId = info.PersonId,
				ShiftStartTime = info.Schedule.ShiftStartTimeForPreviousActivity,
				ShiftEndTime = info.Schedule.ShiftEndTimeForPreviousActivity
			});
		}
	}

}