using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotifier
	{
		void Notify(INotificationMessage messages, params IPerson[] persons);
	}
}