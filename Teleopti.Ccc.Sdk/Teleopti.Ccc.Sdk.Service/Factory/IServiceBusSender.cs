using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public interface IServiceBusSender : IDisposable
    {
        void NotifyServiceBus<TMessage>(TMessage message) where TMessage : RaptorDomainMessage;
        bool EnsureBus();
    }
}