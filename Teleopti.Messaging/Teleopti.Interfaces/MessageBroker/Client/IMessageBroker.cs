using System;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageBroker : IDisposable, IMessageBrokerSender, IMessageBrokerListener
	{
		void StartBrokerService(bool useLongPolling = false);
        void StopBrokerService();
        string ConnectionString { get; set; }
        bool IsAlive { get; }
    }
}