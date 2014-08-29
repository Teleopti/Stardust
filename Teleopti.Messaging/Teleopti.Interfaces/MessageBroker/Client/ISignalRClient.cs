using System;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface ISignalRClient : IDisposable
	{
		bool IsAlive { get; }
		string ServerUrl { get; set; }
		void StartBrokerService(bool useLongPolling = false);
		void Call(string methodName, params object[] args);
		void RegisterCallbacks(Action<Notification> onNotification, Action afterConnectionCreated);
	}
}