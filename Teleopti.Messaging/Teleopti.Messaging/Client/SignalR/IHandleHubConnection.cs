using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.Client.SignalR
{
	public interface IHandleHubConnection
	{
		void StartConnection(Action<Notification> onNotification, bool useLongPolling);
		void CloseConnection();
		bool IsConnected();
	}
}