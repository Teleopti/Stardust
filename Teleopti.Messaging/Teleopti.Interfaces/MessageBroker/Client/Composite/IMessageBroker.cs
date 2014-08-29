using System;

namespace Teleopti.Interfaces.MessageBroker.Client.Composite
{
	public interface IMessageBroker : IDisposable, IMessageBrokerSender, IMessageBrokerListener
	{
		bool IsAlive { get; }
		string ServerUrl { get; set; }
		void StartBrokerService(bool useLongPolling = false);
	}
}