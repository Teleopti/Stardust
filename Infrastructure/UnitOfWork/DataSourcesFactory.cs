using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class UnitOfWorkFactoryNewerUper
	{
		private readonly ICurrentPreCommitHooks _currentPreCommitHooks;
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IUpdatedBy _updatedBy;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;

		public UnitOfWorkFactoryNewerUper(
			ICurrentPreCommitHooks currentPreCommitHooks,
			IEnversConfiguration enversConfiguration,
			ICurrentTransactionHooks transactionHooks,
			ICurrentHttpContext httpContext,
			IUpdatedBy updatedBy,
			ICurrentBusinessUnit businessUnit,
			INestedUnitOfWorkStrategy nestedUnitOfWorkStrategy)
		{
			_currentPreCommitHooks = currentPreCommitHooks;
			_enversConfiguration = enversConfiguration;
			_transactionHooks = transactionHooks;
			_httpContext = httpContext;
			_updatedBy = updatedBy;
			_businessUnit = businessUnit;
			_nestedUnitOfWorkStrategy = nestedUnitOfWorkStrategy;
		}

		public NHibernateUnitOfWorkInterceptor MakeInterceptor()
		{
			return new NHibernateUnitOfWorkInterceptor(_updatedBy, _currentPreCommitHooks);
		}

		public NHibernateUnitOfWorkFactory MakeAppFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant)
		{
			return new NHibernateUnitOfWorkFactory(
				sessionFactory,
				_enversConfiguration.AuditSettingProvider,
				connectionString,
				_transactionHooks,
				tenant,
				_businessUnit,
				_nestedUnitOfWorkStrategy,
				this);
		}

		public AnalyticsUnitOfWorkFactory MakeAnalyticsFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant)
		{
			return new AnalyticsUnitOfWorkFactory(
				sessionFactory,
				connectionString,
				tenant
			);
		}

		public ReadModelUnitOfWorkFactory MakeReadModelFactory(
			string applicationConnectionString
		)
		{
			var factory = new ReadModelUnitOfWorkFactory(_httpContext, applicationConnectionString);
			factory.Configure();
			return factory;
		}
	}

	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly UnitOfWorkFactoryNewerUper _unitOfWorkFactoryNewerUper;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private readonly INHibernateConfigurationCache _nhibernateConfigurationCache;

		public const string AnalyticsDataSourceName = "AnalyticsDatasource";

		public DataSourcesFactory(
			IEnversConfiguration enversConfiguration,
			IDataSourceConfigurationSetter dataSourceConfigurationSetter,
			INHibernateConfigurationCache nhibernateConfigurationCache,
			UnitOfWorkFactoryNewerUper unitOfWorkFactoryNewerUper)
		{
			_enversConfiguration = enversConfiguration;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
			_nhibernateConfigurationCache = nhibernateConfigurationCache;
			_unitOfWorkFactoryNewerUper = unitOfWorkFactoryNewerUper;
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

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString,
			IDictionary<string, string> applicationNhibConfiguration)
		{
			if (applicationNhibConfiguration == null)
				applicationNhibConfiguration = new Dictionary<string, string>();
			applicationNhibConfiguration[Environment.SessionFactoryName] = tenantName;
			applicationNhibConfiguration[Environment.ConnectionString] = applicationConnectionString;
			return createDataSource(applicationNhibConfiguration, statisticConnectionString);
		}

		private IDataSource createDataSource(IDictionary<string, string> applicationConfiguration,
			string statisticConnectionString)
		{
			if (applicationConfiguration == null)
				applicationConfiguration = new Dictionary<string, string>();
			var configuration = createApplicationConfiguration(applicationConfiguration);
			var tenant = configuration.Properties[Environment.SessionFactoryName];
			var applicationConnectionString = configuration.Properties[Environment.ConnectionString];
			NHibernateUnitOfWorkFactory appFactory;
			try
			{
				appFactory = _unitOfWorkFactoryNewerUper.MakeAppFactory(
					buildSessionFactory(configuration),
					applicationConnectionString,
					tenant);
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
				statConfiguration.AddResources(new[]
				{
					"Teleopti.Ccc.Domain.Analytics.AnalyticsPermission.analytics.xml",
					"Teleopti.Ccc.Domain.Analytics.AnalyticsBridgeTimeZone.analytics.xml",
					"Teleopti.Ccc.Domain.Analytics.AnalyticsDate.analytics.xml"
				}, typeof(AnalyticsPermission).Assembly);
				statFactory = _unitOfWorkFactoryNewerUper.MakeAnalyticsFactory(
					buildSessionFactory(statConfiguration),
					statConfiguration.Properties[Environment.ConnectionString],
					tenant);
			}

			var readModel = _unitOfWorkFactoryNewerUper.MakeReadModelFactory(applicationConnectionString);

			return new DataSource(appFactory, statFactory, readModel);
		}

		private ISessionFactory buildSessionFactory(Configuration configuration)
		{
			var cachedSessionFactory = _nhibernateConfigurationCache.GetSessionFactory(configuration);
			if (cachedSessionFactory != null)
				return cachedSessionFactory;
			using (PerformanceOutput.ForOperation(
				$"Building sessionfactory for {configuration.Properties[Environment.SessionFactoryName]}"))
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
			// 2018-01-26
			// Putting down a one beer bet that this will be here on 2020-01-01 // LevelUp
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