using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface IMessageSender
	{
		void Send(Message message);
		void SendMultiple(IEnumerable<Message> messages);
	}
}