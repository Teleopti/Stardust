using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IChannelCreator : IDisposable
    {
        T CreateChannel<T>();
    }
}