using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Teleopti.Messaging.SignalR.Wrappers
{
	[CLSCompliant(false)]
	public class HubProxyWrapper : IHubProxyWrapper
	{
		private readonly IHubProxy _hubProxy;

		public HubProxyWrapper(IHubProxy hubProxy)
		{
			_hubProxy = hubProxy;
		}

		public Task Invoke(string method, params object[] args)
		{
			return _hubProxy.Invoke(method, args);
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			return new SubscriptionWrapper(_hubProxy.Subscribe(eventName));
		}
	}
}