using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client
{
	public class FakeUrl : IMessageBrokerUrl
	{
		private string _url;

		public FakeUrl(string url)
		{
			_url = url;
		}

		public void Configure(string url)
		{
			_url = url;
		}

		public string Url { get { return _url; } }
	}
}