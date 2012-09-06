namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, string mobileNumber);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}