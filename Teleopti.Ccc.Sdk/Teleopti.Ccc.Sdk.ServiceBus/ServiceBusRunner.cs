using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

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
		private ConfigFileDefaultHost _payrollBus;
		[NonSerialized] 
		private ConfigFileDefaultHost _rtaBus;
	
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

			_requestBus = new ConfigFileDefaultHost();
			_requestBus.UseFileBasedBusConfiguration("RequestQueue.config");
			_requestBus.Start<BusBootStrapper>();

			_generalBus = new ConfigFileDefaultHost();
			_generalBus.UseFileBasedBusConfiguration("GeneralQueue.config");
			_generalBus.Start<GeneralBusBootStrapper>();

			_denormalizeBus = new ConfigFileDefaultHost();
			_denormalizeBus.UseFileBasedBusConfiguration("DenormalizeQueue.config");
			_denormalizeBus.Start<DenormalizeBusBootStrapper>();

			_rtaBus = new ConfigFileDefaultHost();
			_rtaBus.UseFileBasedBusConfiguration("RtaQueue.config");
			_rtaBus.Start<RtaBusBootStrapper>();
			
			new PayrollDllCopy(new SearchPath()).CopyPayrollDll();

			_payrollBus = new ConfigFileDefaultHost();
			_payrollBus.UseFileBasedBusConfiguration("PayrollQueue.config");
			_payrollBus.Start<BusBootStrapper>();
			AppDomain.MonitoringIsEnabled = true;
		}

		private bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}
		
		public void Stop()
		{
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
					_requestBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_generalBus != null)
			{
				try
				{
					_generalBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_denormalizeBus != null)
			{
				try
				{
					_denormalizeBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_payrollBus != null)
			{
				try
				{
					_payrollBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			
		}
	}
}