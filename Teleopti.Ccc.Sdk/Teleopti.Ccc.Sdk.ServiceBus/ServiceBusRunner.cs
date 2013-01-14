using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
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
		private FileSystemWatcher _watcher;
		[NonSerialized] 
		private Dictionary<string, DateTime> _copiedFiles;

		[NonSerialized] 
		private ConfigFileDefaultHost _requestBus;
		[NonSerialized] 
		private ConfigFileDefaultHost _generalBus;
		[NonSerialized] 
		private ConfigFileDefaultHost _denormalizeBus;
		[NonSerialized]
		private ConfigFileDefaultHost _payrollBus;

		[NonSerialized] 
		private AppDomain _requestDomain;
		[NonSerialized] 
		private AppDomain _generalDomain;
		[NonSerialized] 
		private AppDomain _denormalizeDomain;
		[NonSerialized]
		private AppDomain _payrollDomain;

		public ServiceBusRunner(Action<Exception> unhandledExceptionHandler, Action<Exception> startupExceptionHandler, Action<int> requestExtraTimeHandler)
		{
			_unhandledExceptionHandler = unhandledExceptionHandler;
			_startupExceptionHandler = startupExceptionHandler;
			_requestExtraTimeHandler = requestExtraTimeHandler;
			_copiedFiles = new Dictionary<string, DateTime>();

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

			startPayrollQueue();

			RunFileWatcher();
		}

		private bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		private void RunFileWatcher()
		{
			try
			{
				var configFolder = Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll.DeployNew\\");
				if (!Directory.Exists(configFolder))
					Directory.CreateDirectory(configFolder);
				_watcher = new FileSystemWatcher(configFolder) {NotifyFilter = NotifyFilters.LastWrite};
				_watcher.Created += OnChanged;
				_watcher.Changed += OnChanged;
				_watcher.EnableRaisingEvents = true;
				_watcher.IncludeSubdirectories = true;
			}
			catch (IOException exception)
			{
				log.Error("An exception was encountered when configuring the custom payroll folder", exception);
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			var totalSleepTime = 0;
			var info = new FileInfo(e.FullPath);

			if (!File.Exists(e.FullPath)
				|| (_copiedFiles.ContainsKey(e.FullPath)
				&& _copiedFiles[e.FullPath] == info.LastWriteTimeUtc
				&& _copiedFiles[e.FullPath].Millisecond == info.LastWriteTimeUtc.Millisecond))
				return;

			while (IsFileLocked(info))
			{
				Thread.Sleep(20);
				totalSleepTime+= 20;
				if (totalSleepTime == 500) return;
			}

			stopPayrollQueue();

			try
			{
				var destination = new SearchPath().Path;
				var source = Directory.GetParent(e.FullPath);
				while (source.Name != "Payroll.DeployNew")
					source = Directory.GetParent(source.ToString());

				CopyFiles(source.ToString(), destination);
			}

			catch (Exception exception)
			{
				log.Error("An exception was encountered when trying to load new payroll dll", exception);
			}

			startPayrollQueue();
		}

		private void CopyFiles(string source, string destination)
		{
			foreach (var folder in Directory.GetDirectories(source))
			{
				var newFolderPath = Path.GetFullPath(destination + Path.GetFileName(folder));

				if (!Directory.Exists(newFolderPath))
					Directory.CreateDirectory(newFolderPath);

				CopyFiles(folder, newFolderPath);
			}
			foreach (var file in Directory.GetFiles(source))
			{
				var totalSleepTime = 0;
				var fileInfo = new FileInfo(file);
				while (IsFileLocked(fileInfo))
				{
					Thread.Sleep(20);
					totalSleepTime += 20;
					if (totalSleepTime == 500) break;
				}
				// if file is still locked, we skip it
				if (totalSleepTime == 500) continue;
				if (file.EndsWith(".xml"))
				{
					var xmlFileDestination = destination;
					while (!xmlFileDestination.EndsWith("\\Payroll"))
						xmlFileDestination = Directory.GetParent(xmlFileDestination).ToString();
					File.Copy(file, Path.GetFullPath(xmlFileDestination + "\\" + Path.GetFileName(file)), true);
				}
				else 
					File.Copy(file, Path.GetFullPath(destination + "\\" + Path.GetFileName(file)), true);

				if (_copiedFiles.ContainsKey(fileInfo.FullName))
					_copiedFiles[fileInfo.FullName] = fileInfo.LastWriteTimeUtc;
				else
					_copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTimeUtc);
			}

		}

		private void startPayrollQueue()
		{
			var e = new Evidence(AppDomain.CurrentDomain.Evidence);
			var setup = AppDomain.CurrentDomain.SetupInformation;

			_payrollDomain = AppDomain.CreateDomain("Pay", e, setup);
			_payrollDomain.UnhandledException += CurrentDomain_UnhandledException;
			_payrollBus = (ConfigFileDefaultHost)_payrollDomain.CreateInstanceFrom(typeof(ConfigFileDefaultHost).Assembly.Location, typeof(ConfigFileDefaultHost).FullName).Unwrap();
			_payrollBus.UseFileBasedBusConfiguration("PayrollQueue.config");
			_payrollBus.Start<BusBootStrapper>();
			log.Info("Starting payroll queue");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void stopPayrollQueue()
		{
			if (_payrollBus != null)
			{
				try
				{
					_payrollBus.Dispose();
					log.Info("Stopping payroll queue to load new dll-file");
				}
				catch (Exception)
				{
				}
			}
			if (_payrollDomain != null)
			{
				AppDomain.Unload(_payrollDomain);
			}
		}

		private static bool IsFileLocked(FileInfo file)
		{
			FileStream stream = null;
			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
			return false;
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
			
			if (_requestDomain != null)
			{
				AppDomain.Unload(_requestDomain);
			}
			if (_generalDomain != null)
			{
				AppDomain.Unload(_generalDomain);
			}
			if (_denormalizeDomain != null)
			{
				AppDomain.Unload(_denormalizeDomain);
			}
			if (_watcher != null)
				_watcher.Dispose();

			stopPayrollQueue();
		}
	}
}