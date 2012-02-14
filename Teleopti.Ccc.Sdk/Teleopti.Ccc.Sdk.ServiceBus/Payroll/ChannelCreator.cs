using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class ChannelCreator : IChannelCreator
    {
        private readonly IList<IDisposable> _channelFactories = new List<IDisposable>();

        public T CreateChannel<T>()
        {
            var channelFactory = new ChannelFactory<T>(typeof (T).Name);
            _channelFactories.Add(channelFactory);

            return channelFactory.CreateChannel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        private void ReleaseManagedResources()
        {
            foreach (var channelFactory in _channelFactories)
            {
                channelFactory.Dispose();
            }
            _channelFactories.Clear();
        }

        private static void ReleaseUnmanagedResources()
        {
        }
    }
}