using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpSender : IMessageSender
	{
		private readonly HttpClientM _client;

		public HttpSender(HttpClientM client)
		{
			_client = client;
		}

		public void Send(Message message)
		{
			_client.Post("MessageBroker/NotifyClients", message);
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_client.Post("MessageBroker/NotifyClientsMultiple", messages);
		}

	}
}