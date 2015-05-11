using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class SubscriptionPassThrough : IBeforeSubscribe
	{
		public void Invoke(Subscription subscription){}
	}
}