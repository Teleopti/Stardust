using System;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface IMessageBrokerComposite : IDisposable, IMessageCreator, IMessageListener, IMessageSender
	{
		bool IsAlive { get; }
		bool IsPollingAlive { get; }
		string ServerUrl { get; set; }
		void StartBrokerService(bool useLongPolling = false);
	}

}