using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	internal interface ISignalWrapper
	{
		Task NotifyClients(Notification notification);
		void StopListening();
		bool IsInitialized();
		void StartListening();
		Task NotifyClients(IEnumerable<Notification> notification);
		Task AddSubscription(Subscription subscription);
		Task RemoveSubscription(string route);
		event Action<Notification> OnNotification;
	}
}