using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	internal interface ISignalWrapper
	{
		void StartHub();
		void StopHub();
		bool IsInitialized();
		Task NotifyClients(Notification notification);
		Task NotifyClients(IEnumerable<Notification> notification);
		Task AddSubscription(Subscription subscription);
		Task RemoveSubscription(string route);
	}
}