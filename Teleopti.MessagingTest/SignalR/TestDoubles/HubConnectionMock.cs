using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class HubConnectionMock :IHubConnectionWrapper
	{
		private readonly IHubProxyWrapper _proxy;
		public HubConnectionMock(IHubProxyWrapper proxy)
		{
			_proxy = proxy;
		}

		public ConnectionState State { get { return ConnectionState.Connected; } }
		public ICredentials Credentials { get; set; }

		public int NumberOfTimesStartWithTransportWasCalled { get; set; }
		public Task Start()
		{
			return TaskHelper.MakeDoneTask();
		}

		public Task Start(IClientTransport transport)
		{
			NumberOfTimesStartWithTransportWasCalled++;
			return TaskHelper.MakeDoneTask();
		}

		public void Stop()
		{
		}

		public void RaiseClosedEvent()
		{
			Closed();
		}

		public event Action Closed;
		public event Action Reconnected;
		public event Action<Exception> Error;

		public IHubProxyWrapper CreateHubProxy(string hubName)
		{
			return _proxy;
		}

		public void DummyToKeepBuildsHappy()
		{
			Reconnected();
			Error(new Exception());
		}
	}
}