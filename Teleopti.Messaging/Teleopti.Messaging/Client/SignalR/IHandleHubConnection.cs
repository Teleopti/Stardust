using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.Client.SignalR
{
	public interface IHandleHubConnection : IDisposable
	{
		void StartConnection(Action<Notification> onNotification, bool useLongPolling);
		bool IsConnected();
	}
}