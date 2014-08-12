using System.Data.SqlClient;
using NHibernate.Cfg;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		public static IDataSourceConfigurationSetter ForTest()
		{
			return new DataSourceConfigurationSetter(false, false, "call", "unit tests", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForTestWithCache()
		{
			return new DataSourceConfigurationSetter(true, false, "call", "unit tests", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForEtl()
		{
			return new DataSourceConfigurationSetter(false, false, "thread_static", "Teleopti.Analytics.ETL.nhib", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForApplicationConfig()
		{
			return new DataSourceConfigurationSetter(false, false, "thread_static", "Teleopti.ApplicationConfiguration", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForSdk()
		{
			return new DataSourceConfigurationSetter(false, false, "thread_static", "Teleopti.Ccc.Sdk.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForServiceBus()
		{
			return new DataSourceConfigurationSetter(false, true, "thread_static", "Teleopti.Ccc.ServiceBus.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForWeb()
		{
			return new DataSourceConfigurationSetter(true, false, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure", "Teleopti.Ccc.Web", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForDesktop()
		{
			return new DataSourceConfigurationSetter(false, false, "thread_static", "Teleopti.Ccc.SmartClientPortal.Shell", new ConfigReader());
		}

		private const string noDataSourceName = "[not set]";

		protected DataSourceConfigurationSetter(bool useSecondLevelCache,
															bool useDistributedTransactionFactory,
															string sessionContext,
															string applicationName,
															IConfigReader configReader)
		{
			UseSecondLevelCache = useSecondLevelCache;
			UseDistributedTransactionFactory = useDistributedTransactionFactory;
			SessionContext = sessionContext;
			ApplicationName = applicationName ?? string.Empty;
			if (!string.IsNullOrEmpty(configReader.AppSettings["latency"]))
				UseLatency = true;
		}

		public bool UseSecondLevelCache { get; private set; }
		public bool UseDistributedTransactionFactory { get; private set; }
		public string SessionContext { get; private set; }
		public string ApplicationName { get; private set; }
		private bool UseLatency { get; set; }

		public void AddDefaultSettingsTo(Configuration nhConfiguration)
		{
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.ConnectionProvider, typeof(TeleoptiDriverConnectionProvider).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.DefaultSchema, "dbo");
			nhConfiguration.SetNamingStrategy(TeleoptiDatabaseNamingStrategy.Instance);
			nhConfiguration.AddAssembly("Teleopti.Ccc.Domain");
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
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.TransactionStrategy,
												 UseDistributedTransactionFactory ?
												 typeof(TeleoptiDistributedTransactionFactory).AssemblyQualifiedName :
												 "NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
			if (!string.IsNullOrEmpty(SessionContext))
				nhConfiguration.SetPropertyIfNotAlreadySet(Environment.CurrentSessionContextClass, SessionContext);

			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SessionFactoryName, noDataSourceName);

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