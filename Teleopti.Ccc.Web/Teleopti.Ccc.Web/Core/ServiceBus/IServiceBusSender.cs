using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Web.Core.ServiceBus
{
    public interface IServiceBusSender : IDisposable
    {
        void NotifyServiceBus<TMessage>(TMessage message) where TMessage : RaptorDomainMessage;
        bool EnsureBus();
    }
}