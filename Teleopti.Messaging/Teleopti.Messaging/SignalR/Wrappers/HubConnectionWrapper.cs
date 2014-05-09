using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Teleopti.Messaging.SignalR.Wrappers
{
	[CLSCompliant(false)]
	public class HubConnectionWrapper : IHubConnectionWrapper
	{
		private readonly HubConnection _hubConnection;

		public HubConnectionWrapper(HubConnection hubConnection)
		{
			_hubConnection = hubConnection;
		}

		public ConnectionState State { get { return _hubConnection.State; } }
		public ICredentials Credentials { get { return _hubConnection.Credentials; } set { _hubConnection.Credentials = value; } }

		public Task Start()
		{
			return _hubConnection.Start();
		}

		public void Stop()
		{
			_hubConnection.Stop();
		}

		public event Action Closed
		{
			add { _hubConnection.Closed += value; }
			remove { _hubConnection.Closed -= value; }
		}

		public event Action Reconnected
		{
			add { _hubConnection.Reconnected += value; }
			remove { _hubConnection.Reconnected -= value; }
		}

		public event Action<Exception> Error
		{
			add { _hubConnection.Error += value; }
			remove { _hubConnection.Error -= value; }
		}

		public IHubProxyWrapper CreateHubProxy(string hubName)
		{
			return new HubProxyWrapper(_hubConnection.CreateHubProxy(hubName));
		}
	}
}