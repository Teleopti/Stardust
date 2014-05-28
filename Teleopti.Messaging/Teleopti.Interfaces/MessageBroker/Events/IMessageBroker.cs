using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IMessageBroker : IDisposable, IMessageBrokerSender, IMessageBrokerListener
	{
		void StartMessageBroker(bool useLongPolling = false);
        void StopMessageBroker();
        string ConnectionString { get; set; }
        bool IsConnected { get; }
    }
}