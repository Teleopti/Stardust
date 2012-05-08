using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using Autofac;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Web.Core.ServiceBus
{
    public class ServiceBusSender : IServiceBusSender
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ServiceBusSender));
        private IContainer _customHost;
        private static readonly object LockObject = new object();
        private bool _isRunning;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void MoveThatBus()
        {
            bool enabled;
            var enabledString = ConfigurationManager.AppSettings["EnableAutoDenyAbsenceRequest"];
            if (bool.TryParse(enabledString, out enabled) && enabled)
            {
                lock (LockObject)
                {
                    if (!_isRunning)
                    {
                        try
                        {
                        	var builder = new ContainerBuilder();
                        	_customHost = builder.Build();

                        	new OnewayRhinoServiceBusConfiguration()
								.UseAutofac(_customHost)
								.UseStandaloneConfigurationFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Teleopti.Ccc.Sdk.ServiceBus.Client.config"))
								.Configure();
							
                            _isRunning = true;
                            if (Logger.IsInfoEnabled)
                                Logger.Info("Client started");
                        }
                        catch (Exception exception)
                        {
                            Logger.Error("The Teleopti Service Bus could not be started, due to an exception.",
                                         exception);
                        }
                    }
                }
            }
        }

        public void NotifyServiceBus<TMessage>(TMessage message) where TMessage : RaptorDomainMessage
        {
            var bus = _customHost.Resolve<IOnewayBus>();

            if (Logger.IsDebugEnabled)
                Logger.Debug(string.Format(CultureInfo.InvariantCulture,
                                           "Sending message with identity: {0} (Data source = {1})",
                                           message.Identity, message.Datasource));

            bus.Send(message);
        }

        public bool EnsureBus()
        {
            if (!_isRunning) MoveThatBus();
            return _isRunning;
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
            if (_customHost!= null)
                _customHost.Dispose();
            _customHost = null;
            Logger.Info("Transport disposed");
        }
    }
}