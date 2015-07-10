using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.SignalR
{
	public class SignalRSender : IMessageSender
	{
		private readonly ISignalRClient _client;

		public SignalRSender(ISignalRClient client)
		{
			_client = client;
		}

		public void Send(Message message)
		{
			_client.Call("NotifyClients", message);
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_client.Call("NotifyClientsMultiple", messages);
		}
	}
}