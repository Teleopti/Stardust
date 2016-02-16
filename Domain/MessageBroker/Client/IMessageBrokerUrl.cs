namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface IMessageBrokerUrl
	{
		void Configure(string url);
		string Url { get; }
	}
}