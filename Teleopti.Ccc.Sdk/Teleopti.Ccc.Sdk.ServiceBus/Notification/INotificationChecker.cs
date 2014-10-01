using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface INotificationChecker
	{
		string SmsMobileNumber(IPerson person);
		NotificationType NotificationType { get; }
	}
}