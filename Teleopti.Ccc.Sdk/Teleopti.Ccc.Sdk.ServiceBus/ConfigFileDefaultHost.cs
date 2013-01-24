using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Rhino.ServiceBus.Config;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.SqlQueues.Config;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class ConfigFileDefaultHost : MarshalByRefObject, IApplicationHost
	{
		private readonly ILog logger = LogManager.GetLogger(typeof(DefaultHost));
		private string assemblyName;
		private AbstractBootStrapper bootStrapper;
		private IStartable startable;
		private string bootStrapperName;
		private BusConfigurationSection hostConfiguration;

		public IStartable Bus
		{
			get { return startable; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public void SetBootStrapperTypeName(string typeName)
		{
			bootStrapperName = typeName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper")]
		public void Start<TBootStrapper>()
			where TBootStrapper : AbstractBootStrapper
		{
			SetBootStrapperTypeName(typeof(TBootStrapper).FullName);
			Start(typeof(TBootStrapper).Assembly.FullName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public void Start(string asmName)
		{
			InitializeBus(asmName);

			startable.Start();

			bootStrapper.EndStart();
		}

		private void InitializeBus(string asmName)
		{
			string logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

			XmlConfigurator.ConfigureAndWatch(new FileInfo(logfile));

			assemblyName = asmName;

			CreateBootStrapper();

			log4net.GlobalContext.Properties["BusName"] = bootStrapper.GetType().Namespace;

			InitializeContainer();

			bootStrapper.BeginStart();

			logger.Debug("Starting bus");
			startable = bootStrapper.GetInstance<IStartable>();
		}

		private void InitializeContainer()
		{
			bootStrapper.InitializeContainer();
			if (hostConfiguration==null) throw new AbandonedMutexException();
			if (hostConfiguration != null)
			{
				bootStrapper.UseConfiguration(hostConfiguration);
			}
		}

		private void CreateBootStrapper()
		{
			logger.DebugFormat("Loading {0}", assemblyName);
			var assembly = Assembly.Load(assemblyName);

			Type bootStrapperType = null;

			if (string.IsNullOrEmpty(bootStrapperName) == false)
				bootStrapperType = assembly.GetType(bootStrapperName);

			var queueConnection = ConfigurationManager.ConnectionStrings["Queue"];
			if (queueConnection!=null)
			{
				QueueConnectionStringContainer.ConnectionString = queueConnection.ConnectionString;
			}

			bootStrapperType = bootStrapperType ??
				GetAutoBootStrapperType(assembly);
			try
			{
				bootStrapper = (AbstractBootStrapper)Activator.CreateInstance(bootStrapperType);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Failed to create " + bootStrapperType + ".", e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "strapper")]
		private static Type GetAutoBootStrapperType(Assembly assembly)
		{
			var bootStrappers = assembly.GetTypes()
				.Where(x => typeof(AbstractBootStrapper).IsAssignableFrom(x) && x.IsAbstract == false)
				.ToArray();

			if (bootStrappers.Length == 0)
				throw new InvalidOperationException("Could not find a boot strapper for " + assembly);

			if (bootStrappers.Length > 1)
			{
				throw new InvalidOperationException("Found more than one boot strapper for " + assembly +
					" you need to specify which boot strapper to use: " + Environment.NewLine +
					string.Join(Environment.NewLine, bootStrappers.Select(x => x.FullName).ToArray()));
			}

			return bootStrappers[0];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			if (bootStrapper != null)
				bootStrapper.Dispose();
			if (startable != null)
				startable.Dispose();
		}

		public override object InitializeLifetimeService()
		{
			return null; //singleton
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public void InitialDeployment(string asmName, string user)
		{
			InitializeBus(asmName);
			bootStrapper.ExecuteDeploymentActions(user);

			bootStrapper.ExecuteEnvironmentValidationActions();
		}

		public void UseFileBasedBusConfiguration(string fileName)
		{
			var config =
				ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName))).GetSection("rhino.esb");
			if (config==null)
				throw new ArgumentNullException(fileName,"test");
			hostConfiguration = config as BusConfigurationSection;
		}
	}
}
