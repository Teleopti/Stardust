using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ShiftEventPublisher : IShiftEventPublisher
	{
		private readonly IRtaDecoratingEventPublisher _eventPublisher;

		public ShiftEventPublisher(IRtaDecoratingEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info)
		{
			publishShiftStartEvent(info);
			publishShiftEndEvent(info);
		}

		private void publishShiftStartEvent(StateInfo info)
		{
			if (info.Schedule.ShiftStarted())
			{
				_eventPublisher.Publish(info, new PersonShiftStartEvent
				{
					PersonId = info.PersonId,
					ShiftStartTime = info.CurrentShiftStartTime,
					ShiftEndTime = info.CurrentShiftEndTime
				});
			}
		}

		private void publishShiftEndEvent(StateInfo info)
		{
			if (info.Schedule.ShiftEnded())
			{
				_eventPublisher.Publish(info, new PersonShiftEndEvent
				{
					PersonId = info.PersonId,
					ShiftStartTime = info.ShiftStartTimeForPreviousActivity,
					ShiftEndTime = info.ShiftEndTimeForPreviousActivity
				});
			}
		}

	}

}