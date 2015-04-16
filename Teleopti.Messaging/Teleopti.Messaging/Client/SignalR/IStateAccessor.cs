using System;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.Messaging.Client.SignalR
{
	public interface IStateAccessor
	{
		void WithConnection(Action<IHubConnectionWrapper> action);
		void WithProxy(Action<IHubProxyWrapper> action);
		void IfProxyConnected(Action<IHubProxyWrapper> action);
	}
}