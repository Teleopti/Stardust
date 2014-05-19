using System;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public interface IStateAccessor
	{
		void WithConnection(Action<IHubConnectionWrapper> action);
		void WithProxy(Action<IHubProxyWrapper> action);
		void IfProxyConnected(Action<IHubProxyWrapper> action);
	}
}