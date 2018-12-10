using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IAdherenceDayStartEventPublisher
	{
		void Publish(Context context);
	}

	public class NoAdherenceDayStartEventPublisher : IAdherenceDayStartEventPublisher
	{
		public void Publish(Context context)
		{
		}
	}

	public class AdherenceDayStartEventPublisher : IAdherenceDayStartEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public AdherenceDayStartEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			if (context.Schedule.ShiftStartsInOneHour())
				_eventPublisher.Publish(
					new PersonAdherenceDayStartEvent
					{
						PersonId = context.PersonId,
						ActivityName = context.Schedule.CurrentActivityName(),
						ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,
						StateName = context.State.StateGroupName(),
						Timestamp = context.Time,
						RuleName = context.State.RuleName(),
						RuleColor = context.State.RuleDisplayColor(),
						Adherence = context.Adherence.Adherence(),
						BelongsToDate = new DateOnly(context.Time)
					}
				);
		}
	}
}