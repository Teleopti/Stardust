using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IBeforeSubscribe
	{
		void Invoke(Subscription subscription);
	}
}