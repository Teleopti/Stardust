using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Messaging.Client.SignalR
{
	public class InProcessMessageSender : IMessageSender
	{
		private readonly IMessageBrokerServer _messageBrokerServer;

		public InProcessMessageSender(IMessageBrokerServer messageBrokerServer)
		{
			_messageBrokerServer = messageBrokerServer;
		}

		public void Send(Message message)
		{
			_messageBrokerServer.NotifyClients(message);
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_messageBrokerServer.NotifyClientsMultiple(messages);
		}
	}
}