using System;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeSignalRClient : ISignalRClient
	{

		public void Configure(string url)
		{
			Url = url;
		}

		public string Url { get; private set; }

		public bool IsAlive { get { return true; } }

		public void StartBrokerService(bool useLongPolling = false)
		{
		}

		public void Call(string methodName, params object[] args)
		{
		}

		public void RegisterCallbacks(Action<Interfaces.MessageBroker.Notification> onNotification, Action afterConnectionCreated)
		{
		}

		public void Dispose()
		{
		}
	}
}