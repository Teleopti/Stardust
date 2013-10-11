using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class ChannelCreator : IChannelCreator
    {
        private readonly IList<IDisposable> _channelFactories = new List<IDisposable>();

        public T CreateChannel<T>()
        {
            var channelFactory = new ChannelFactory<T>(typeof (T).Name);
            _channelFactories.Add(channelFactory);
			channelFactory.Faulted += onFactoryOnFaulted;
			
            return channelFactory.CreateChannel();
        }

	    private void onFactoryOnFaulted(object sender, EventArgs e)
	    {
		    var factory = sender as IChannelFactory;
		    if (factory == null) return;

		    factory.Faulted -= onFactoryOnFaulted;
		    _channelFactories.Remove((IDisposable) factory);
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