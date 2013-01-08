using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	[Serializable]
	public class ServiceBusRunner
	{
		private readonly Action<Exception> _unhandledExceptionHandler;
		private readonly Action<Exception> _startupExceptionHandler;
		private readonly Action<int> _requestExtraTimeHandler;
		private static readonly ILog log = LogManager.GetLogger(typeof(ServiceBusRunner));

		[NonSerialized] 
		private ConfigFileDefaultHost _requestBus;
		[NonSerialized] 
		private ConfigFileDefaultHost _generalBus;
		[NonSerialized] 
		private ConfigFileDefaultHost _denormalizeBus;

		[NonSerialized] 
		private AppDomain _requestDomain;
		[NonSerialized] 
		private AppDomain _generalDomain;
		[NonSerialized] 
		private AppDomain _denormalizeDomain;

		public ServiceBusRunner(Action<Exception> unhandledExceptionHandler, Action<Exception> startupExceptionHandler, Action<int> requestExtraTimeHandler)
		{
			_unhandledExceptionHandler = unhandledExceptionHandler;
			_startupExceptionHandler = startupExceptionHandler;
			_requestExtraTimeHandler = requestExtraTimeHandler;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.EventLog.WriteEntry(System.String,System.Diagnostics.EventLogEntryType)")]
		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception ?? new InvalidOperationException();

			log.Error("An unhandled exception occurred.", exception);
			if (_unhandledExceptionHandler!=null)
			{
				_unhandledExceptionHandler.Invoke(exception);
			}
		}

		public void Start()
		{
			try
			{
				HostServiceStart();
			}
			catch (InvalidOperationException exception)
			{
				log.Error("An exception was encountered upon starting the Teleopti Service Bus.",exception);
				if (_startupExceptionHandler!=null)
				{
					_startupExceptionHandler.Invoke(exception);
				}
				throw;
			}
		}

		private void HostServiceStart()
		{
			if (_requestExtraTimeHandler!=null)
			{
				_requestExtraTimeHandler.Invoke(60000);
			}
			XmlConfigurator.Configure(new FileInfo("log4net.config"));

			ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			var e = new Evidence(AppDomain.CurrentDomain.Evidence);
			var setup = AppDomain.CurrentDomain.SetupInformation;

			log.Error("Ska starta request.");
			_requestDomain = AppDomain.CreateDomain("Req", e, setup);
			_requestDomain.UnhandledException += CurrentDomain_UnhandledException;
			//_requestBus = new ConfigFileDefaultHost();
			_requestBus = (ConfigFileDefaultHost)_requestDomain.CreateInstanceFrom(typeof(ConfigFileDefaultHost).Assembly.Location, typeof(ConfigFileDefaultHost).FullName).Unwrap();
			_requestBus.UseFileBasedBusConfiguration("RequestQueue.config");
			_requestBus.Start<BusBootStrapper>();

			_generalDomain = AppDomain.CreateDomain("Gen", e, setup);
			_generalDomain.UnhandledException += CurrentDomain_UnhandledException;
			//_generalBus = new ConfigFileDefaultHost();
			_generalBus = (ConfigFileDefaultHost)_generalDomain.CreateInstanceFrom(typeof(ConfigFileDefaultHost).Assembly.Location, typeof(ConfigFileDefaultHost).FullName).Unwrap();
			_generalBus.UseFileBasedBusConfiguration("GeneralQueue.config");
			_generalBus.Start<BusBootStrapper>();

			_denormalizeDomain = AppDomain.CreateDomain("Den", e, setup);
			_denormalizeDomain.UnhandledException += CurrentDomain_UnhandledException;
			//_denormalizeBus = new ConfigFileDefaultHost();
			_denormalizeBus = (ConfigFileDefaultHost)_denormalizeDomain.CreateInstanceFrom(typeof(ConfigFileDefaultHost).Assembly.Location, typeof(ConfigFileDefaultHost).FullName).Unwrap();
			_denormalizeBus.UseFileBasedBusConfiguration("DenormalizeQueue.config");
			_denormalizeBus.Start<DenormalizeBusBootStrapper>();
		}

		private bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public void Stop()
		{
			log.Warn("Ska stanna.");
			HostServiceStop();
		}

		private void HostServiceStop()
		{
			if (_requestExtraTimeHandler != null)
			{
				_requestExtraTimeHandler.Invoke(60000);
			}
			DisposeBusHosts();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void DisposeBusHosts()
		{
			
			if (_requestBus != null)
			{
				try
				{
					log.Warn("Ska stanna request.");
					_requestBus.Dispose();
					log.Warn("Stannade request.");
				}
				catch (Exception)
				{
				}
			}
			if (_generalBus != null)
			{
				try
				{
					log.Warn("Ska stanna general.");
					_generalBus.Dispose();
					log.Warn("Stannade general.");
				}
				catch (Exception)
				{
				}
			}
			if (_denormalizeBus != null)
			{
				try
				{
					log.Warn("Ska stanna denormalize.");
					_denormalizeBus.Dispose();
					log.Warn("Stannade denormalize.");
				}
				catch (Exception)
				{
				}
			}
			if (_requestDomain != null)
			{
				AppDomain.Unload(_requestDomain);
				log.Warn("Stannade dom�n request.");
			}
			if (_generalDomain != null)
			{
				AppDomain.Unload(_generalDomain);
				log.Warn("Stannade dom�n general.");
			}
			if (_denormalizeDomain != null)
			{
				AppDomain.Unload(_denormalizeDomain);
				log.Warn("Stannade dom�n denormalize.");
			}
		}
	}
}