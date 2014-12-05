using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface INotifier
	{
		void Notify(INotificationMessage messages, IPerson person);
		void Notify(INotificationMessage messages, IPerson[] persons);
	}
}