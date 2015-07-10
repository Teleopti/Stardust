using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpSender : IMessageSender
	{
		private readonly HttpRequests _client;

		public HttpSender(HttpRequests client)
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