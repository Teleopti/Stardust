namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageBrokerUrl
	{
		void Configure(string url);
		string Url { get; }
	}
}