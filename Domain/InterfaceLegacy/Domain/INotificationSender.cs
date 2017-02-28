namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, NotificationHeader notificationHeader);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}