using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public interface IBeforeSubscribe
	{
		void Invoke(Subscription subscription);
	}
}