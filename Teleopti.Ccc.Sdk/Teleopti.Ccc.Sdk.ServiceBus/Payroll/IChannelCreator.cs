namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IChannelCreator
    {
        T CreateChannel<T>();
    }
}