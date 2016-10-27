using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Rhino.ServiceBus.Config;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.SqlQueues.Config;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ConfigFileDefaultHost : IApplicationHost
	{
		private readonly ILog logger = LogManager.GetLogger(typeof(ConfigFileDefaultHost));
		private string assemblyName;
		private readonly AbstractBootStrapper _bootStrapper;
		private IStartable startable;
		private string bootStrapperName;
		private BusConfigurationSection hostConfiguration;
		private readonly string _directory;

		public ConfigFileDefaultHost(string fileName, AbstractBootStrapper bootStrapper)
			: this(fileName, bootStrapper, AppDomain.CurrentDomain.BaseDirectory)
		{
		}

		public ConfigFileDefaultHost(string fileName, AbstractBootStrapper bootStrapper, string directory)
		{
			_directory = directory;
			_bootStrapper = bootStrapper;
			useFileBasedBusConfiguration(Path.Combine(_directory, fileName));
		}

		private void useFileBasedBusConfiguration(string filePath)
		{
			var config =
				ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(filePath)).GetSection("rhino.esb");
			if (config == null)
				throw new ApplicationException($"Unable to find section rhino.esb in file {filePath}");
			hostConfiguration = config as BusConfigurationSection;
		}

		public IStartable Bus
		{
			get { return startable; }
		}

		public void SetBootStrapperTypeName(string type)
		{
			bootStrapperName = type;
		}

		public void Start(string assembly)
		{
			initializeBus(assembly);

			startable.Start();

			_bootStrapper.EndStart();
		}

		public void Start()
		{
			SetBootStrapperTypeName(_bootStrapper.GetType().FullName);
			Start(_bootStrapper.GetType().Assembly.FullName);
		}

		private void initializeBus(string asmName)
		{
			string logfile = Path.Combine(_directory, "log4net.config");

			XmlConfigurator.ConfigureAndWatch(new FileInfo(logfile));

			assemblyName = asmName;

			CreateBootStrapper();

			GlobalContext.Properties["BusName"] = _bootStrapper.GetType().Namespace;

			InitializeContainer();

			_bootStrapper.BeginStart();

			logger.Debug("Starting bus");
			startable = _bootStrapper.GetInstance<IStartable>();
		}

		private void InitializeContainer()
		{
			_bootStrapper.InitializeContainer();
			if (hostConfiguration==null) throw new AbandonedMutexException();
			if (hostConfiguration != null)
			{
				_bootStrapper.UseConfiguration(hostConfiguration);
			}
		}

		private void CreateBootStrapper()
		{
			var queueConnection = ConfigurationManager.ConnectionStrings["Queue"];
			if (queueConnection!=null)
			{
				QueueConnectionStringContainer.ConnectionString = queueConnection.ConnectionString;
			}
		}

		public void Dispose()
		{
			if (_bootStrapper != null)
				_bootStrapper.Dispose();
			if (startable != null)
				startable.Dispose();
		}

		public void InitialDeployment(string asmName, string user)
		{
			initializeBus(asmName);
			_bootStrapper.ExecuteDeploymentActions(user);
			_bootStrapper.ExecuteEnvironmentValidationActions();
		}

	}
}
