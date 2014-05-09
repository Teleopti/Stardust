using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IMessageBroker : IDisposable, IMessageBrokerSender, IMessageBrokerListener
	{
		void StartMessageBroker();
        void StopMessageBroker();
        string ConnectionString { get; set; }
        bool IsInitialized { get; }
    }
}