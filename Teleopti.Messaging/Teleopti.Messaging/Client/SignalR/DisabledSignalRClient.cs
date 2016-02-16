using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Messaging.Client.SignalR
{
	public class DisabledSignalRClient : ISignalRClient
	{
		public void Dispose()
		{
		}

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

		public void RegisterCallbacks(Action<Message> onMessage, Action afterConnectionCreated)
		{
		}
	}
}