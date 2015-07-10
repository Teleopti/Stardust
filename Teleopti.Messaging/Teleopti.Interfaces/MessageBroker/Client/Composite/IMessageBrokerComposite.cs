using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client.Composite
{
	public interface IMessageBrokerComposite : IDisposable, IMessageCreator, IMessageListener, IMessageSender
	{
		bool IsAlive { get; }
		bool IsPollingAlive { get; }
		string ServerUrl { get; set; }
		void StartBrokerService(bool useLongPolling = false);
	}

}