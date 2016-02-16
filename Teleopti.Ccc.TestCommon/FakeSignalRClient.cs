using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSignalRClient : ISignalRClient
	{

		public Action<Message> RegisteredCallback { get; set; }

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
			RegisteredCallback = onMessage;
		}

		public void Dispose()
		{
		}
	}
}