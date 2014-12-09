namespace Teleopti.Interfaces.Domain
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, NotificationHeader notificationHeader);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}