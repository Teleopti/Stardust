using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ActivityEventPublisher : IActivityEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ActivityEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info)
		{
			if (
				info.NewState.ScheduledId != info.PreviousState.ScheduledId &&
				info.CurrentActivity != null
				)
			{
				_eventPublisher.Publish(new PersonActivityStartEvent
				{
					PersonId = info.NewState.PersonId,
					StartTime = info.CurrentActivity.StartDateTime,
					Name = info.CurrentActivity.Name,
					BusinessUnitId = info.NewState.BusinessUnitId
				});
			}
		}
	}
}