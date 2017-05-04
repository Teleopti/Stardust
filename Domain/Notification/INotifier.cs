using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotifier
	{
		Task<bool> Notify(INotificationMessage messages, params IPerson[] persons);
	}
}