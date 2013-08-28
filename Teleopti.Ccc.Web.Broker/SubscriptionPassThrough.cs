using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class SubscriptionPassThrough : IBeforeSubscribe
	{
		public void Invoke(Subscription subscription){}
	}
}