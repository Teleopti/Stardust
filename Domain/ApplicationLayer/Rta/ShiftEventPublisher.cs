using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ShiftEventPublisher : IShiftEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ShiftEventPublisher(IEventPopulatingPublisher eventPublisher)
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
			if (info.IsScheduled && !info.WasScheduled)
			{
				_eventPublisher.Publish(new PersonShiftStartEvent
				{
					PersonId = info.PersonId,
					ShiftStartTime = info.CurrentShiftStartTime,
					ShiftEndTime = info.CurrentShiftEndTime,
					BusinessUnitId = info.BusinessUnitId
				});
			}
		}

		private void publishShiftEndEvent(StateInfo info)
		{
			if (!info.IsScheduled && info.WasScheduled)
			{
				_eventPublisher.Publish(new PersonShiftEndEvent
				{
					PersonId = info.PersonId,
					ShiftStartTime = info.ShiftStartTimeForPreviousActivity,
					ShiftEndTime = info.ShiftEndTimeForPreviousActivity,
					BusinessUnitId = info.BusinessUnitId
				});
			}

		}

	}
}