using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	public interface ISignalBrokerCommands
	{
		void NotifyClients(Notification notification);
		void NotifyClients(IEnumerable<Notification> notification);
		void AddSubscription(Subscription subscription);
		void RemoveSubscription(string route);
	}
}