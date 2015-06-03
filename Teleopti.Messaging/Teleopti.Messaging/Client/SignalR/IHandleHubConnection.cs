using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.Client.SignalR
{
	public interface IHandleHubConnection : IDisposable
	{
		void StartConnection(Action<Message> onNotification, bool useLongPolling);
		bool IsConnected();
	}
}