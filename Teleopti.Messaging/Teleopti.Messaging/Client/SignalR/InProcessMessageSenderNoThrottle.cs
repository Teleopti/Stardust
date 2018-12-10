using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Messaging.Client.SignalR
{
	public class InProcessMessageSenderNoThrottle : IMessageSender
	{
		private readonly IMessageBrokerServer _messageBrokerServer;

		public InProcessMessageSenderNoThrottle(IMessageBrokerServer messageBrokerServer)
		{
			_messageBrokerServer = messageBrokerServer;
		}

		public void Send(Message message) =>
			_messageBrokerServer.NotifyClients(message);

		public void SendMultiple(IEnumerable<Message> messages) =>
			_messageBrokerServer.NotifyClientsMultiple(messages);
	}
}