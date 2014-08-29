using System.Collections.Generic;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageSender
	{
		void Send(Notification notification);
		void SendMultiple(IEnumerable<Notification> notifications);
	}
}