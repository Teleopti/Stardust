using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	
	public interface INotificationSender
	{

		void SendNotification(INotificationMessage message, NotificationHeader receiverInfo);
		
		void SetConfigReader(INotificationConfigReader notificationConfigReader);
	}
}