using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client
{
	public class NoMessageSender : IMessageSender
	{
		public void Send(Message message)
		{
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
		}
	}
}