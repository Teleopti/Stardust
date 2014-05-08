using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public interface IDoHubProxyCalls
	{
		void WithProxy(Action<IHubProxyWrapper> action);
	}

	public interface ISignalConnectionHandler : IDoHubProxyCalls
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