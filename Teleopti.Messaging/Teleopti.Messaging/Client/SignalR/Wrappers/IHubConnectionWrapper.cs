using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Teleopti.Messaging.Client.SignalR.Wrappers
{
	[CLSCompliant(false)]
	public interface IHubConnectionWrapper
	{
		ConnectionState State { get; }
		ICredentials Credentials { get; set; }
		Task Start();
		Task Start(IClientTransport transport);
		void Stop();
		event Action Closed;
		event Action Reconnected;
		event Action<Exception> Error;
		IHubProxyWrapper CreateHubProxy(string hubName);
	}
}