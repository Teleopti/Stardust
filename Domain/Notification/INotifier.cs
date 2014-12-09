using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotifier
	{
		void Notify(INotificationMessage messages, IPerson person);
		void Notify(INotificationMessage messages, IPerson[] persons);
	}
}