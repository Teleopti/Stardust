using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly ICurrentPersistCallbacks _persistCallbacks;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private readonly ICurrentHttpContext _httpContext;
		private readonly Func<IMessageBrokerComposite> _messageBroker;

		public const string AnalyticsDataSourceName = "AnalyticsDatasource";

		public DataSourcesFactory(
			IEnversConfiguration enversConfiguration,
			ICurrentPersistCallbacks persistCallbacks,
			IDataSourceConfigurationSetter dataSourceConfigurationSetter, 
			ICurrentHttpContext httpContext, 
			Func<IMessageBrokerComposite> messageBroker)
		{
			_enversConfiguration = enversConfiguration;
			_persistCallbacks = persistCallbacks;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
			_httpContext = httpContext;
			_messageBroker = messageBroker ?? (() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging);
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return createDataSource(applicationNhibConfiguration, statisticConnectionString);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			var applicationNhibConfiguration = new Dictionary<string, string>();
			applicationNhibConfiguration[Environment.SessionFactoryName] = tenantName;
			applicationNhibConfiguration[Environment.ConnectionString] = applicationConnectionString;
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

		private IDataSource createDataSource(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			if (applicationNhibConfiguration == null)
				applicationNhibConfiguration = new Dictionary<string, string>();
			NHibernateUnitOfWorkMatrixFactory statFactory;
			var appConfig = createApplicationConfiguration(applicationNhibConfiguration);
			var applicationConnectionString = appConfig.Properties[Environment.ConnectionString];
			var sessionFactory = buildSessionFactory(appConfig);
			var appFactory = new NHibernateUnitOfWorkFactory(
				sessionFactory,
				_enversConfiguration.AuditSettingProvider, 
				applicationConnectionString,
				_persistCallbacks, 
				_messageBroker);

			if (!string.IsNullOrEmpty(statisticConnectionString))
			{
				var statConfiguration = createStatisticConfiguration(statisticConnectionString);
				statFactory = new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(statConfiguration), statConfiguration.Properties[Environment.ConnectionString]);
			}
			else
			{
				statFactory = null;
			}

			var readModel = new ReadModelUnitOfWorkFactory(_httpContext, applicationConnectionString);
			readModel.Configure();

			return new DataSource(appFactory, statFactory, readModel);
		}

		private static ISessionFactory buildSessionFactory(Configuration nhConf)
		{
			using (PerformanceOutput.ForOperation("Building sessionfactory for " + nhConf.Properties[Environment.SessionFactoryName]))
			{
				var sessionFactory = nhConf.BuildSessionFactory();
				sessionFactory.Statistics.IsStatisticsEnabled = true;
				return sessionFactory;
			}
		}

		private Configuration createApplicationConfiguration(IDictionary<string, string> settings)
		{
			var appCfg = new Configuration();
			appCfg.SetProperties(settings);
			setDefaultValuesOnApplicationConf(appCfg);
			return appCfg;
		}

		private Configuration createStatisticConfiguration(string connectionString)
		{
			//REMOVE ME LATER!!!!!!!!!!!!!!!!/((
			Log4NetConfiguration.SetConnectionString(connectionString);
			////////////////////////////////////
			// ^^ how much later are we talking?
			using (PerformanceOutput.ForOperation("Configuring statistic db"))
			{
				var statCfg = new Configuration()
						 .SetProperty(Environment.ConnectionString, connectionString)
						 .SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider")
						 .SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver")
						 .SetProperty(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName)
						 .SetProperty(Environment.SessionFactoryName, AnalyticsDataSourceName)
						 .SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
				_dataSourceConfigurationSetter.AddApplicationNameToConnectionString(statCfg);
				return statCfg;
			}
		}

		private void setDefaultValuesOnApplicationConf(Configuration cfg)
		{
			_dataSourceConfigurationSetter.AddDefaultSettingsTo(cfg);
			_enversConfiguration.Configure(cfg);
		}
	}
}
