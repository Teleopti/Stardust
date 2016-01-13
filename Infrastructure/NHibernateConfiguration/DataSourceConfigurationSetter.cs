using System.Data.SqlClient;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.SqlAzure;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		public static IDataSourceConfigurationSetter ForTest()
		{
			return new DataSourceConfigurationSetter(false, "call", "unit tests", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForTestWithCache()
		{
			return new DataSourceConfigurationSetter(true, "call", "unit tests", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForEtl()
		{
			return new DataSourceConfigurationSetter(false, "thread_static", "Teleopti.Wfm.Etl", new ConfigReader());
		}
		
		public static IDataSourceConfigurationSetter ForApplicationConfig()
		{
			return new DataSourceConfigurationSetter(false, "thread_static", "Teleopti.ApplicationConfiguration", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForSdk()
		{
			return new DataSourceConfigurationSetter(false, "thread_static", "Teleopti.Wfm.Sdk.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForServiceBus()
		{
			return new DataSourceConfigurationSetter(false, "thread_static", "Teleopti.Wfm.ServiceBus.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForWeb()
		{
			return new DataSourceConfigurationSetter(true, typeof(TeleoptiSessionContext).AssemblyQualifiedName, "Teleopti.Wfm.Web", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForDesktop()
		{
			return new DataSourceConfigurationSetter(false, "thread_static", "Teleopti.Wfm.SmartClientPortal.Shell", new ConfigReader());
		}

		public const string NoDataSourceName = "[not set]";

		protected DataSourceConfigurationSetter(
			bool useSecondLevelCache,
			string sessionContext,
			string applicationName,
			IConfigReader configReader)
		{
			UseSecondLevelCache = useSecondLevelCache;
			SessionContext = sessionContext;
			ApplicationName = applicationName ?? string.Empty;
			if (!string.IsNullOrEmpty(configReader.AppConfig("latency")))
				UseLatency = true;
		}

		public bool UseSecondLevelCache { get; private set; }
		public string SessionContext { get; private set; }
		public string ApplicationName { get; private set; }
		private bool UseLatency { get; set; }

		public void AddDefaultSettingsTo(Configuration nhConfiguration)
		{
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.DefaultSchema, "dbo");
			nhConfiguration.AddAssembly(typeof(Person).Assembly);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			if (UseSecondLevelCache)
			{
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.CacheProvider, typeof(RtMemoryCacheProvider).AssemblyQualifiedName);
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.UseSecondLevelCache, "true");
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.UseQueryCache, "true");
			}
			else
			{
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.UseSecondLevelCache, "false");
			}
			if (UseLatency)
			{
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.ConnectionDriver, typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName);
			}
			else
			{
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.ConnectionDriver, typeof(SqlAzureClientDriverWithLogRetries).AssemblyQualifiedName);
			}
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.TransactionStrategy, typeof(ReliableAdoNetTransactionFactory).AssemblyQualifiedName);
			if (!string.IsNullOrEmpty(SessionContext))
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.CurrentSessionContextClass, SessionContext);

			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SessionFactoryName, NoDataSourceName);

			AddApplicationNameToConnectionString(nhConfiguration);
		}

		public void AddApplicationNameToConnectionString(Configuration nhConfiguration)
		{
			var connString = nhConfiguration.GetProperty(Environment.ConnectionString);
			var connStringObj = new SqlConnectionStringBuilder(connString);
			if (connStringObj.ToString().Contains("Application Name")) 
				return;
			connStringObj.ApplicationName = ApplicationName;
			nhConfiguration.SetProperty(Environment.ConnectionString, connStringObj.ToString());
		}
	}
}