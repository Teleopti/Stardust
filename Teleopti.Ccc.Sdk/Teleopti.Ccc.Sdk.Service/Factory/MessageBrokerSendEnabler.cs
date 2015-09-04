﻿using System;
using System.Configuration;
using log4net;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class MessageBrokerSendEnabler : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageBrokerSendEnabler));
        private readonly IMessageBrokerComposite _messaging;
        private readonly static object LockObject = new object();

        public MessageBrokerSendEnabler()
        {
            if (StateHolderReader.IsInitialized)
            {
                _messaging = StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging;

                if (_messaging != null && !MessageBrokerReceiveEnabled)
                {
                    lock (LockObject)
                    {
                        try
                        {
							var useLongPolling = StateHolderReader.Instance.StateReader.ApplicationScopeData.AppSettings.GetSettingValue("MessageBrokerLongPolling", bool.Parse);
							_messaging.StartBrokerService(useLongPolling);
                        }
                        catch (BrokerNotInstantiatedException ex)
                        {
                            Logger.Warn("Message broker could not be started.", ex);
                        }
                    }
                }
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
            if (_messaging != null && _messaging.IsAlive && !MessageBrokerReceiveEnabled)
            {
                lock (LockObject)
                {
                    _messaging.Dispose();
                }
            }
        }
    }
}