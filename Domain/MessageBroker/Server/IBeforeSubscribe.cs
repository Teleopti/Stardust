namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IBeforeSubscribe
	{
		void Invoke(Subscription subscription);
	}
}