namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, string receiver);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}