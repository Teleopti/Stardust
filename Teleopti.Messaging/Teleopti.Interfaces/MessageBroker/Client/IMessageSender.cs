using System.Collections.Generic;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageSender
	{
		void Send(Message message);
		void SendMultiple(IEnumerable<Message> notifications);
	}
}