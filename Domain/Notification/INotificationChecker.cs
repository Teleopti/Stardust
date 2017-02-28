using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotificationChecker
	{
		NotificationType NotificationType();
		NotificationLookup Lookup();
	}
}