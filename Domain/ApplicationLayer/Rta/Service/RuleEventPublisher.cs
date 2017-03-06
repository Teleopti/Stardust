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
				PersonId = context.PersonId
			});
		}
	}
}