using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface ISignalRClient : IDisposable, IMessageBrokerUrl
	{
		bool IsAlive { get; }
		void StartBrokerService(bool useLongPolling = false);
		void Call(string methodName, params object[] args);
		void RegisterCallbacks(Action<Message> onMessage, Action afterConnectionCreated);
	}
}