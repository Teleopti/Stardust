using System;
using System.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class MessageBrokerSendEnabler : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageBrokerSendEnabler));
        private readonly IMessageBroker _messaging;
        private readonly static object LockObject = new object();

        public MessageBrokerSendEnabler()
        {
            if (StateHolderReader.IsInitialized)
            {
                _messaging = StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging;

                if (_messaging != null && !MessageBrokerDisabled && !MessageBrokerReceiveEnabled)
                {
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

        private static bool MessageBrokerDisabled
        {
            get
            {
                var messageBrokerDisabled = false;
                var messageBrokerDisabledStringValue = ConfigurationManager.AppSettings["MessageBrokerDisabled"];
                if (!string.IsNullOrEmpty(messageBrokerDisabledStringValue))
                {
                    if (!bool.TryParse(messageBrokerDisabledStringValue, out messageBrokerDisabled))
                        messageBrokerDisabled = false;
                }
                return messageBrokerDisabled;
            }
        }

        public static bool MessageBrokerReceiveEnabled
        {
            get
            {
                var messageBrokerReceiveEnabled = false;
                var messageBrokerReceiveEnabledStringValue = ConfigurationManager.AppSettings["MessageBrokerReceiveEnabled"];
                if (!string.IsNullOrEmpty(messageBrokerReceiveEnabledStringValue))
                {
                    if (!bool.TryParse(messageBrokerReceiveEnabledStringValue, out messageBrokerReceiveEnabled))
                        messageBrokerReceiveEnabled = false;
                }
                return messageBrokerReceiveEnabled;
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
            if (_messaging != null && _messaging.IsConnected && !MessageBrokerReceiveEnabled)
            {
                lock (LockObject)
                {
                    _messaging.StopMessageBroker();
                }
            }
        }
    }
}