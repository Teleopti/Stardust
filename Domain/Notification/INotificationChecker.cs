using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotificationChecker
	{
		string SmsMobileNumber(IPerson person);
		NotificationType NotificationType();
		string EmailSender { get; }
	}
}