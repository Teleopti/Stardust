using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using log4net;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class MessageBrokerSendEnabler : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageBrokerSendEnabler));
        private readonly IMessageBroker _messaging;
        private readonly static object LockObject = new object();
    	private readonly bool _wasStartedBefore;

    	public MessageBrokerSendEnabler()
        {
            if (StateHolderReader.IsInitialized)
            {
                _messaging = StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging;
				if (_messaging != null)
				{
					if (_messaging.IsConnected)
					{
						_wasStartedBefore = true;
						return;
					}
                
                    lock (LockObject)
                    {
                        try
                        {
                            _messaging.StartMessageBroker();
                        }
                        catch (BrokerNotInstantiatedException ex)
                        {
                            Logger.Warn("Message broker could not be started.", ex);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
			if (_messaging != null && _messaging.IsConnected && !_wasStartedBefore)
            {
                lock (LockObject)
                {
                    _messaging.StopMessageBroker();
                }
            }
        }
    }
}