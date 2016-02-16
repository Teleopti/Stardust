using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Messaging.Client
{
	public class MutableUrl : IMessageBrokerUrl
	{
		private string _url;

		public void Configure(string url)
		{
			_url = url;
		}

		public string Url { get { return _url; } }
	}
}