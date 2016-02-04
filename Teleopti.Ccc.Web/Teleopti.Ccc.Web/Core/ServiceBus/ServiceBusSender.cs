using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using log4net;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Web.Core.ServiceBus
{
	public class ServiceBusSender : IServiceBusSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceBusSender));
		private IContainer _customHost;
		private static readonly object LockObject = new object();
		private bool _isRunning;

		private void MoveThatBus()
		{
			lock (LockObject)
			{
				if (!_isRunning)
				{
					try
					{
						var builder = new ContainerBuilder();
						_customHost = builder.Build();

						Rhino.ServiceBus.SqlQueues.Config.QueueConnectionStringContainer.ConnectionString =
							ConfigurationManager.ConnectionStrings["Queue"].ConnectionString;

						new OnewayRhinoServiceBusConfiguration()
							.UseAutofac(_customHost)
							.UseStandaloneConfigurationFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teleopti.Ccc.Web.ServiceBus.Client.config"))
							.Configure();

						_isRunning = true;
						if (Logger.IsInfoEnabled)
							Logger.Info("Client started");
					}
					catch (Exception exception)
					{
						Logger.Error("The Teleopti Service Bus could not be started, due to an exception.", exception);
					}
				}
			}
		}

		protected virtual T Resolve<T>()
		{
			return _customHost.Resolve<T>();
		}

		public void Send(bool throwOnNoBus, params object[] message)
		{
			if (message.Length==0)return;
			if (!EnsureBus())
			{
				if (throwOnNoBus)
					throw new ApplicationException("The outgoing queue for the service bus is not available. Cannot send the message " + message.GetType().Name);
				return;
			}

			var bus = Resolve<IOnewayBus>();

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

		protected virtual bool EnsureBus()
		{
			if (!_isRunning) MoveThatBus();
			return _isRunning;
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

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		protected virtual void ReleaseManagedResources()
		{
			if (_customHost != null)
				_customHost.Dispose();
			_customHost = null;
			Logger.Info("Transport disposed");
		}
	}
}