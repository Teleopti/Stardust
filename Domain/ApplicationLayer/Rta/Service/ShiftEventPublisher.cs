using System.Runtime.Remoting.Messaging;
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
			if (!info.Schedule.ShiftStarted())
				return;
			_eventPublisher.Publish(info, new PersonShiftStartEvent
			{
				PersonId = info.Person.PersonId,
				ShiftStartTime = info.Schedule.CurrentShiftStartTime,
				ShiftEndTime = info.Schedule.CurrentShiftEndTime
			});
		}

		private void publishShiftEndEvent(StateInfo info)
		{
			if (!info.Schedule.ShiftEnded())
				return;
			_eventPublisher.Publish(info, new PersonShiftEndEvent
			{
				PersonId = info.Person.PersonId,
				ShiftStartTime = info.Schedule.ShiftStartTimeForPreviousActivity,
				ShiftEndTime = info.Schedule.ShiftEndTimeForPreviousActivity
			});
		}
	}

}