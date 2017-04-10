using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Autofac;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class ServiceBusSender : IServiceBusSender
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ServiceBusSender));
        private IContainer _customHost;
        private static readonly object LockObject = new object();
        private bool _isRunning;

	    private void MoveThatBus()
	    {
		    lock (LockObject)
		    {
			    if (_isRunning) return;
			    try
			    {
				    var builder = new ContainerBuilder();
				    builder.RegisterType<DateOnlyPeriodSerializer>().As<ICustomElementSerializer>();
				    builder.RegisterType<LargeGuidCollectionSerializer>().As<ICustomElementSerializer>();
				    _customHost = builder.Build();

				    Rhino.ServiceBus.SqlQueues.Config.QueueConnectionStringContainer.ConnectionString =
					    StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["Queue"];

				    new OnewayRhinoServiceBusConfiguration()
					    .UseAutofac(_customHost)
					    .UseStandaloneConfigurationFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
					                                                 "Teleopti.Ccc.Sdk.ServiceBus.Client.config"))
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

		public void Send(bool throwOnNoBus, params object[] message)
		{
			if (message.Length == 0) return;
			if (!EnsureBus())
			{
				if (throwOnNoBus)
					throw new ApplicationException("The outgoing queue for the service bus is not available. Cannot send the message " + message.GetType().Name);
				return;
			}

            var bus = _customHost.Resolve<IOnewayBus>();

            if (Logger.IsDebugEnabled)
			{
				var identity = "<unknown>";
				var datasource = "<unknown>";
				var raptorDomainMessage = message.First() as ILogOnContext;
				if (raptorDomainMessage != null)
				{
					datasource = raptorDomainMessage.LogOnDatasource;
				}
				Logger.Debug(string.Format(CultureInfo.InvariantCulture,
										   "Sending message with identity: {0} (Data source = {1})",
										   identity, datasource));
			}

            bus.Send(message);
        }

	    private bool EnsureBus()
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