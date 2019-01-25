using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly UnitOfWorkFactoryFactory _unitOfWorkFactoryFactory;
		private readonly IUpdatedBy _updatedBy;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private readonly MemoryNHibernateConfigurationCache _nhibernateConfigurationCache;

		public const string AnalyticsDataSourceName = "AnalyticsDatasource";

		public DataSourcesFactory(
			IEnversConfiguration enversConfiguration,
			IDataSourceConfigurationSetter dataSourceConfigurationSetter,
			MemoryNHibernateConfigurationCache nhibernateConfigurationCache,
			UnitOfWorkFactoryFactory unitOfWorkFactoryFactory,
			IUpdatedBy updatedBy)
		{
			_enversConfiguration = enversConfiguration;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
			_nhibernateConfigurationCache = nhibernateConfigurationCache;
			_unitOfWorkFactoryFactory = unitOfWorkFactoryFactory;
			_updatedBy = updatedBy;
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
				appFactory = _unitOfWorkFactoryFactory.MakeAppFactory(
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
				statFactory = _unitOfWorkFactoryFactory.MakeAnalyticsFactory(
					buildSessionFactory(statConfiguration),
					statConfiguration.Properties[Environment.ConnectionString],
					tenant);
			}

			var readModel = _unitOfWorkFactoryFactory.MakeReadModelFactory(applicationConnectionString);

			return new DataSource(appFactory, statFactory, readModel);
		}

		private ISessionFactory buildSessionFactory(Configuration configuration)
		{
			var cachedSessionFactory = _nhibernateConfigurationCache.GetSessionFactory(configuration);
			if (cachedSessionFactory != null)
				return cachedSessionFactory;
			using (PerformanceOutput.ForOperation(
				$"Building session factory for {configuration.Properties[Environment.SessionFactoryName]}"))
			{
				var sessionFactory = configuration.BuildSessionFactory();
				_nhibernateConfigurationCache.StoreSessionFactory(configuration, sessionFactory);
				return sessionFactory;
			}
		}

		private Configuration createApplicationConfiguration(IDictionary<string, string> settings)
		{
			var cachedConfiguration = _nhibernateConfigurationCache.GetConfiguration(settings);
			if (cachedConfiguration != null)
				return cachedConfiguration;

			var configuration = new Configuration()
				.SetProperties(settings);
			
			_dataSourceConfigurationSetter.AddDefaultSettingsTo(configuration);

			_enversConfiguration.Configure(configuration, _updatedBy);
			_nhibernateConfigurationCache.StoreConfiguration(settings, configuration);
			return configuration;
		}

		private Configuration createStatisticConfiguration(string connectionString, string tenant)
		{
			var configuration = new Configuration()
				.SetProperty(Environment.ConnectionString, connectionString)
				.SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider")
				.SetProperty(Environment.ConnectionDriver, typeof(ResilientSql2008ClientDriver).AssemblyQualifiedName)
				.SetProperty(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName)
				.SetProperty(Environment.SessionFactoryName, tenant + "_" + AnalyticsDataSourceName)
				.SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName);

			_dataSourceConfigurationSetter.AddApplicationNameToConnectionString(configuration);

			return configuration;
		}
	}
}