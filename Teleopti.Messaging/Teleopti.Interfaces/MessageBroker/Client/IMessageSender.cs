using System.Collections.Generic;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageSender
	{
		void SendNotification(Notification notification);
		void SendNotifications(IEnumerable<Notification> notifications);
	}
}