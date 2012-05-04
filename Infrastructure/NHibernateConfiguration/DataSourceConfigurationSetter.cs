using NHibernate.Cfg;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		private readonly bool _useSecondLevelCache;
		private readonly bool _useDistributedTransactionFactory;
		private readonly string _sessionContext;
		public const string NoDataSourceName = "[not set]";

		public DataSourceConfigurationSetter(bool useSecondLevelCache, bool useDistributedTransactionFactory, string sessionContext)
		{
			_useSecondLevelCache = useSecondLevelCache;
			_useDistributedTransactionFactory = useDistributedTransactionFactory;
			_sessionContext = sessionContext;
		}

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
			if (_useSecondLevelCache)
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
			                            _useDistributedTransactionFactory ? 
												 typeof (AdoNetWithDistributedTransactionFactory).AssemblyQualifiedName : 
												 "NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
			if (!string.IsNullOrEmpty(_sessionContext))
				nhConfiguration.SetProperty(Environment.CurrentSessionContextClass, _sessionContext);
		}
	}
}