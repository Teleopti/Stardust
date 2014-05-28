using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	public interface IHandleHubConnection
	{
		void StartConnection(Action<Notification> onNotification, bool useLongPolling);
		void CloseConnection();
		bool IsConnected();
	}
}