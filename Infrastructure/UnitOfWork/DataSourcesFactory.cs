using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Environment = NHibernate.Cfg.Environment;
using Teleopti.Ccc.Domain.Common.Logging;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly IEnumerable<IMessageSender> _messageSenders;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private readonly ICurrentHttpContext _httpContext;
		private readonly Func<IMessageBrokerComposite> _messageBroker;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DataSourcesFactory));

		public const string AnalyticsDataSourceName = "AnalyticsDatasource";

		public DataSourcesFactory(IEnversConfiguration enversConfiguration, IEnumerable<IMessageSender> messageSenders,
			IDataSourceConfigurationSetter dataSourceConfigurationSetter, ICurrentHttpContext httpContext, Func<IMessageBrokerComposite> messageBroker)
		{
			_enversConfiguration = enversConfiguration;
			_messageSenders = messageSenders;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
			_httpContext = httpContext;
			_messageBroker = messageBroker ?? (() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging);
		}

		private static string isSqlServerOnline(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");

			var buildedConnectionString = new SqlConnectionStringBuilder(connectionString);
			try
			{
				using (var sqlConnection = new SqlConnection(buildedConnectionString.ToString()))
				{
					Logger.DebugFormat("The connection timeout is set to {0}.", sqlConnection.ConnectionTimeout);
					sqlConnection.Open();
					return string.Empty;
				}
			}
			catch (SqlException sqlException)
			{
				Logger.ErrorFormat("Database {1} on server {0} is unavailable. Exception details below.", buildedConnectionString.DataSource, buildedConnectionString.InitialCatalog);
				Logger.Error("Could not connect to data source.", sqlException);
				return sqlException.Message;
			}
		}

		public bool TryCreate(XElement nhibernateConfiguration, out IDataSource dataSource)
		{
			var nhProperties = createApplicationProperties(nhibernateConfiguration);
			string connectionString;
			if (nhProperties.TryGetValue(Environment.ConnectionString, out connectionString))
			{
				var resultOfOnline = isSqlServerOnline(connectionString);
				if (string.IsNullOrEmpty(resultOfOnline))
				{
					var matrixElement = nhibernateConfiguration.Elements("matrix").Single();
					var matrixConnstring = matrixElement.Element("connectionString").Value;
					dataSource = createDataSource(nhProperties, matrixConnstring);
					return true;
				}
			}
			dataSource = null;
			return false;
		}

		public IDataSource Create(string applicationDataSourceName, string applicationConnectionString, string statisticConnectionString)
		{
			var settings = new Dictionary<string, string>
			{
				{
					Environment.ConnectionString,
					applicationConnectionString
				},
				{
					Environment.SessionFactoryName,
					applicationDataSourceName
				}
			};
			return createDataSource(settings, statisticConnectionString);
		}

		public IDataSource Create(IDictionary<string, string> settings, string statisticConnectionString)
		{
			return createDataSource(settings, statisticConnectionString);
		}

		private IDataSource createDataSource(IDictionary<string, string> settings, string statisticConnectionString)
		{
			NHibernateUnitOfWorkMatrixFactory statFactory;
			var appConfig = createApplicationConfiguration(settings);
			var applicationConnectionString = appConfig.Properties[Environment.ConnectionString];
			var appFactory = new NHibernateUnitOfWorkFactory(buildSessionFactory(appConfig),
				_enversConfiguration.AuditSettingProvider, applicationConnectionString, _messageSenders, _messageBroker);

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
				return nhConf.BuildSessionFactory();
			}
		}

		private Configuration createApplicationConfiguration(IDictionary<string, string> settings)
		{
			var appCfg = new Configuration();
			appCfg.SetProperties(settings);
			setDefaultValuesOnApplicationConf(appCfg);
			return appCfg;
		}

		private static IDictionary<string, string> createApplicationProperties(XElement nhibernateConfiguration)
		{
			var sessionFactory = nhibernateConfiguration.Descendants(((XNamespace)"urn:nhibernate-configuration-2.2") + "session-factory").SingleOrDefault();
			if (sessionFactory == null)
				throw new DataSourceException("Missing session-factory element!");
			var sessionFactoryProperties = sessionFactory.Elements();
			var ret = sessionFactoryProperties.ToDictionary(p => p.Attribute("name").Value, p => p.Value);
			ret[Environment.SessionFactoryName] = sessionFactory.Attribute("name").Value;
			return ret;
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
