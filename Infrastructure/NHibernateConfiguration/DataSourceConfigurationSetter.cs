using System.Data.SqlClient;
using NHibernate.Cfg;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		public const string NoDataSourceName = "[not set]";

		public static IDataSourceConfigurationSetter ForTest = 
			new DataSourceConfigurationSetter(false, false, "call", "unit tests");
		public static IDataSourceConfigurationSetter ForEtl = 
			new DataSourceConfigurationSetter(false, false, "thread_static", "ETL tool");
		public static IDataSourceConfigurationSetter ForApplicationConfig = 
			new DataSourceConfigurationSetter(false, false, "thread_static", "Application Config");
		public static IDataSourceConfigurationSetter ForSdk = 
			new DataSourceConfigurationSetter(false, false, "thread_static", "SDK");
		public static IDataSourceConfigurationSetter ForServiceBus = 
			new DataSourceConfigurationSetter(false, true, "thread_static", "Service bus");
		public static IDataSourceConfigurationSetter ForWeb = 
			new DataSourceConfigurationSetter(true, false, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure", "Web mytime");
		public static IDataSourceConfigurationSetter ForDesktop = 
			new DataSourceConfigurationSetter(false, false, "thread_static", "Desktop");

		protected DataSourceConfigurationSetter(bool useSecondLevelCache, 
															bool useDistributedTransactionFactory, 
															string sessionContext,
															string applicationName)
		{
			UseSecondLevelCache = useSecondLevelCache;
			UseDistributedTransactionFactory = useDistributedTransactionFactory;
			SessionContext = sessionContext;
			ApplicationName = applicationName ?? string.Empty;
		}

		public bool UseSecondLevelCache { get; private set; }
		public bool UseDistributedTransactionFactory { get; private set; }
		public string SessionContext { get; private set; }
		public string ApplicationName { get; private set; }

		public void AddDefaultSettingsTo(Configuration nhConfiguration)
		{
			nhConfiguration.SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			nhConfiguration.SetProperty(Environment.ConnectionProvider, typeof(TeleoptiDriverConnectionProvider).AssemblyQualifiedName);
			nhConfiguration.SetProperty(Environment.DefaultSchema, "dbo");
			nhConfiguration.SetProperty(Environment.SessionFactoryName, NoDataSourceName);
			nhConfiguration.SetNamingStrategy(TeleoptiDatabaseNamingStrategy.Instance);
			nhConfiguration.AddAssembly("Teleopti.Ccc.Domain");
			nhConfiguration.SetProperty(Environment.ProxyFactoryFactoryClass,
			                            typeof (FasterProxyFactoryFactory).AssemblyQualifiedName);
			nhConfiguration.SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			if (UseSecondLevelCache)
			{
				nhConfiguration.SetProperty(Environment.CacheProvider, "NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache");
				nhConfiguration.SetProperty(Environment.UseSecondLevelCache, "true");
				nhConfiguration.SetProperty(Environment.UseQueryCache, "true");
			}
			else
			{
				nhConfiguration.SetProperty(Environment.UseSecondLevelCache, "false");
			}
			nhConfiguration.SetProperty(Environment.TransactionStrategy,
			                            UseDistributedTransactionFactory ? 
												 typeof (TeleoptiDistributedTransactionFactory).AssemblyQualifiedName : 
												 "NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
			if (!string.IsNullOrEmpty(SessionContext))
				nhConfiguration.SetProperty(Environment.CurrentSessionContextClass, SessionContext);

			fixApplicationNameOnConnectionString(nhConfiguration);
		}

		private void fixApplicationNameOnConnectionString(Configuration nhConfiguration)
		{
			var connString = nhConfiguration.GetProperty(Environment.ConnectionString);
			var connStringObj = new SqlConnectionStringBuilder(connString);
			if (!connStringObj.ToString().Contains("Application Name"))
			{
				connStringObj.ApplicationName = ApplicationName;
				nhConfiguration.SetProperty(Environment.ConnectionString, connStringObj.ToString());
			}
		}
	}
}