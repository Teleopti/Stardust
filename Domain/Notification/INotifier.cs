using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotifier
	{
		void Notify(INotificationMessage messages, params IPerson[] persons);
	}
}