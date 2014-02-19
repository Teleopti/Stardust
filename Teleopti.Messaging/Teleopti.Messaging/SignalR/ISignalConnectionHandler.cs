using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	public interface ISignalConnectionHandler
	{
		void StartConnection();
		void CloseConnection();
		bool IsInitialized();
		Task NotifyClients(Notification notification);
		Task NotifyClients(IEnumerable<Notification> notification);
		Task AddSubscription(Subscription subscription);
		Task RemoveSubscription(string route);
	}
}