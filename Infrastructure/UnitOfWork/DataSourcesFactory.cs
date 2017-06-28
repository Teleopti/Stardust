using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly ICurrentPreCommitHooks _currentPreCommitHooks;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private readonly ICurrentHttpContext _httpContext;
		private readonly INHibernateConfigurationCache _nhibernateConfigurationCache;

		public const string AnalyticsDataSourceName = "AnalyticsDatasource";

		public DataSourcesFactory(
			IEnversConfiguration enversConfiguration, 
			ICurrentTransactionHooks transactionHooks, 
			IDataSourceConfigurationSetter dataSourceConfigurationSetter, 
			ICurrentHttpContext httpContext, 
			INHibernateConfigurationCache nhibernateConfigurationCache, ICurrentPreCommitHooks currentPreCommitHooks)
		{
			_enversConfiguration = enversConfiguration;
			_transactionHooks = transactionHooks;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
			_httpContext = httpContext;
			_nhibernateConfigurationCache = nhibernateConfigurationCache;
			_currentPreCommitHooks = currentPreCommitHooks;
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return createDataSource(applicationNhibConfiguration, statisticConnectionString);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			var applicationNhibConfiguration = new Dictionary<string, string>
			{
				[Environment.SessionFactoryName] = tenantName,
				[Environment.ConnectionString] = applicationConnectionString
			};
			return createDataSource(applicationNhibConfiguration, statisticConnectionString);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			if (applicationNhibConfiguration == null)
				applicationNhibConfiguration = new Dictionary<string, string>();
			applicationNhibConfiguration[Environment.SessionFactoryName] = tenantName;
			applicationNhibConfiguration[Environment.ConnectionString] = applicationConnectionString;
			return createDataSource(applicationNhibConfiguration, statisticConnectionString);
		}

		private IDataSource createDataSource(IDictionary<string, string> applicationConfiguration, string statisticConnectionString)
		{
			if (applicationConfiguration == null)
				applicationConfiguration = new Dictionary<string, string>();
			var configuration = createApplicationConfiguration(applicationConfiguration);
			var tenant = configuration.Properties[Environment.SessionFactoryName];
			var applicationConnectionString = configuration.Properties[Environment.ConnectionString];
			NHibernateUnitOfWorkFactory appFactory;
			try
			{
				var sessionFactory = buildSessionFactory(configuration);
				appFactory = new NHibernateUnitOfWorkFactory(
					sessionFactory,
					_enversConfiguration.AuditSettingProvider,
					applicationConnectionString,
					_transactionHooks,
					_currentPreCommitHooks,
					tenant
				);
			}
			catch (Exception)
			{
				_nhibernateConfigurationCache.Clear(applicationConfiguration);
				throw;
			}

			AnalyticsUnitOfWorkFactory statFactory = null;
			if (!string.IsNullOrEmpty(statisticConnectionString))
			{
				var statConfiguration = createStatisticConfiguration(statisticConnectionString, tenant);
				statConfiguration.AddResources(new []
				{
					"Teleopti.Ccc.Domain.Analytics.AnalyticsPermission.analytics.xml",
					"Teleopti.Ccc.Domain.Analytics.AnalyticsBridgeTimeZone.analytics.xml",
					"Teleopti.Ccc.Domain.Analytics.AnalyticsDate.analytics.xml"
				}, typeof(AnalyticsPermission).Assembly);
				statFactory = new AnalyticsUnitOfWorkFactory(
					buildSessionFactory(statConfiguration),
					statConfiguration.Properties[Environment.ConnectionString], 
					tenant, 
					_transactionHooks
					);
			}

			var readModel = new ReadModelUnitOfWorkFactory(_httpContext, applicationConnectionString);
			readModel.Configure();

			return new DataSource(appFactory, statFactory, readModel);
		}

		private ISessionFactory buildSessionFactory(Configuration configuration)
		{
			var cachedSessionFactory = _nhibernateConfigurationCache.GetSessionFactory(configuration);
			if (cachedSessionFactory != null)
				return cachedSessionFactory;
			using (PerformanceOutput.ForOperation($"Building sessionfactory for {configuration.Properties[Environment.SessionFactoryName]}"))
			{
				var sessionFactory = configuration.BuildSessionFactory();
				sessionFactory.Statistics.IsStatisticsEnabled = true;
				_nhibernateConfigurationCache.StoreSessionFactory(configuration, sessionFactory);
				return sessionFactory;
			}
		}

		private Configuration createApplicationConfiguration(IDictionary<string, string> settings)
		{
			var cachedConfiguration = _nhibernateConfigurationCache.GetConfiguration(settings);
			if (cachedConfiguration != null)
				return cachedConfiguration;
			var appCfg = new Configuration()
				.SetProperties(settings);
			setDefaultValuesOnApplicationConf(appCfg);
			_nhibernateConfigurationCache.StoreConfiguration(settings, appCfg);
			return appCfg;
		}

		private Configuration createStatisticConfiguration(string connectionString, string tenant)
		{
			// 2013-10-03
			//REMOVE ME LATER!!!!!!!!!!!!!!!!/((
			Log4NetConfiguration.SetConnectionString(connectionString);
			////////////////////////////////////
			// 2014-10-24
			// ^^ how much later are we talking?
			// 2016-03-07
			// Maybe for David's bday?
			var statCfg = new Configuration()
				.SetProperty(Environment.ConnectionString, connectionString)
				.SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider")
				.SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver")
				.SetProperty(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName)
				.SetProperty(Environment.SessionFactoryName, tenant + "_" + AnalyticsDataSourceName)
				.SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName)
				;
			_dataSourceConfigurationSetter.AddApplicationNameToConnectionString(statCfg);
			return statCfg;
		}

		private void setDefaultValuesOnApplicationConf(Configuration cfg)
		{
			_dataSourceConfigurationSetter.AddDefaultSettingsTo(cfg);
			_enversConfiguration.Configure(cfg);
		}
	}
}
