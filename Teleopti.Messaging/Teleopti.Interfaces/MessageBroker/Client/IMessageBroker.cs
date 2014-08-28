using System;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageBroker : IDisposable, ISignalRClient, IMessageBrokerSender, IMessageBrokerListener
	{
        string ConnectionString { get; set; }
    }
}