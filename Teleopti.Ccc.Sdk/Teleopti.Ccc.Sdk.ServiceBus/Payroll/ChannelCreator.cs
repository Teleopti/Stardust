using System.ServiceModel;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class ChannelCreator : IChannelCreator
    {
        public T CreateChannel<T>()
        {
            var channelFactory = new ChannelFactory<T>(typeof (T).Name);
			return channelFactory.CreateChannel();
        }
	}
}