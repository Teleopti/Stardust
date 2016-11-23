using System.Collections.Generic;
using System.Threading.Tasks;
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
			Task.Run(() => _messageBrokerServer.NotifyClients(message));
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			Task.Run(() => _messageBrokerServer.NotifyClientsMultiple(messages));
		}
	}
}