namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, string to);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}