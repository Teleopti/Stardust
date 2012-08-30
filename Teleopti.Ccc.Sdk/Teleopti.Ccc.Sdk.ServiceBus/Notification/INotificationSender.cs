using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, string mobileNumber);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}