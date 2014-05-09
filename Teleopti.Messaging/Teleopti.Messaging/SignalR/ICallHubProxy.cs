using System;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public interface ICallHubProxy
	{
		void WithProxy(Action<IHubProxyWrapper> action);
		void IfProxyConnected(Action<IHubProxyWrapper> action);
	}
}