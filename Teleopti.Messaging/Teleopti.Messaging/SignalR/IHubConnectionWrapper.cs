using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Teleopti.Messaging.SignalR
{
	[CLSCompliant(false)]
	public interface IHubConnectionWrapper
	{
		ConnectionState State { get; }
		ICredentials Credentials { get; set; }
		Task Start();
		void Stop();
		event Action Closed;
		event Action Reconnected;
		event Action<Exception> Error;
		IHubProxy CreateHubProxy(string hubName);
	}
}