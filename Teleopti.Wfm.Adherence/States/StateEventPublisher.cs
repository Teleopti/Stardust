using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class StateEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		
		public StateEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info)
		{
			if (!info.State.StateChanged()) return;

			_eventPublisher.Publish(new PersonStateChangedEvent
			{
				BelongsToDate = info.Schedule.BelongsToDate,
				PersonId = info.PersonId,
				Timestamp = info.Time,
				StateName = info.State.StateGroupName(),
				StateGroupId = info.State.StateGroupId(),
				ActivityName = info.Schedule.CurrentActivityName(),
				ActivityColor = info.Schedule.CurrentActivity()?.DisplayColor,
				RuleName = info.State.RuleName(),
				RuleColor = info.State.RuleDisplayColor(),
				Adherence = info.Adherence.Adherence(),
			});
		}
	}
}