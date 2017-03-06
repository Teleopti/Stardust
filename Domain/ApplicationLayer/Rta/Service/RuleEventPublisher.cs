using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RuleEventPublisher
	{
		private readonly IEventPopulatingPublisher _publisher;

		public RuleEventPublisher(IEventPopulatingPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Publish(Context context)
		{

			if (!context.State.RuleChanged()) return;

			_publisher.Publish(new PersonRuleChangedEvent
			{
				PersonId = context.PersonId,
				Timestamp = context.CurrentTime,
				BelongsToDate = context.Schedule.BelongsToDate,
				StateName = context.State.StateGroupName(),
				StateGroupId = context.State.StateGroupId(),
				RuleName = context.State.RuleName(),
				Adherence = context.Adherence.Adherence,
				ActivityName = context.Schedule.CurrentActivityName(),
				ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,
				RuleColor = context.State.RuleDisplayColor()
			});
		}
	}
}