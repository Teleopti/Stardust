using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface IServiceBusSender : IDisposable
    {
        void NotifyServiceBus<TMessage>(TMessage message) where TMessage : RaptorDomainMessage;
        bool EnsureBus();
    }

	public interface IMessageBrokerEnablerFactory
	{
		IDisposable NewMessageBrokerEnabler();
	}
}