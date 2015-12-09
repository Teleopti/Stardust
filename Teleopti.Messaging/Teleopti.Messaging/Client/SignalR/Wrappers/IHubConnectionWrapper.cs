using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Teleopti.Messaging.Client.SignalR.Wrappers
{
	public interface IHubConnectionWrapper
	{
#pragma warning disable CS3003 // Type is not CLS-compliant
		ConnectionState State { get; }
#pragma warning restore CS3003 // Type is not CLS-compliant
		ICredentials Credentials { get; set; }
		Task Start();
#pragma warning disable CS3001 // Argument type is not CLS-compliant
		Task Start(IClientTransport transport);
#pragma warning restore CS3001 // Argument type is not CLS-compliant
		void Stop();
		event Action Closed;
		event Action Reconnected;
		event Action<Exception> Error;
		IHubProxyWrapper CreateHubProxy(string hubName);
	}
}