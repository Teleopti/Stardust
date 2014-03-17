using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Environment = NHibernate.Cfg.Environment;
using Teleopti.Ccc.Domain.Common.Logging;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly IEnumerable<IMessageSender> _messageSenders;
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DataSourcesFactory));

		public const string NoDataSourceName = "[not set]";

		public DataSourcesFactory(IEnversConfiguration enversConfiguration, IEnumerable<IMessageSender> messageSenders, IDataSourceConfigurationSetter dataSourceConfigurationSetter)
		{
			_enversConfiguration = enversConfiguration;
			_messageSenders = messageSenders;
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
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
					var authenticationSettings = createAuthenticationSettings(nhibernateConfiguration);
					var matrixElement = nhibernateConfiguration.Elements("matrix").Single();
					var matrixNameElement = matrixElement.Attribute("name");
					var matrixName = matrixNameElement!=null ? matrixNameElement.Value : NoDataSourceName;
					var matrixConnstring = matrixElement.Element("connectionString").Value;
					dataSource = createDataSource(nhProperties, matrixConnstring,matrixName, authenticationSettings);
					return true;
				}
			}
			dataSource = null;
			return false;
		}

		public IDataSource Create(IDictionary<string, string> settings, string statisticConnectionString)
		{
			var authenticationSettings = createDefaultAuthenticationSettings();
			return createDataSource(settings, statisticConnectionString, NoDataSourceName, authenticationSettings);
		}

		private IDataSource createDataSource(IDictionary<string, string> settings, string statisticConnectionString, string statisticName, IAuthenticationSettings authenticationSettings)
		{
			NHibernateUnitOfWorkMatrixFactory statFactory;
			var appConfig = createApplicationConfiguration(settings);
			var appFactory = new NHibernateUnitOfWorkFactory(buildSessionFactory(appConfig), _enversConfiguration.AuditSettingProvider, appConfig.Properties[Environment.ConnectionString], _messageSenders);
			if (!string.IsNullOrEmpty(statisticConnectionString))
			{
				var statConfiguration = createStatisticConfiguration(statisticConnectionString, statisticName);
				statFactory = new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(statConfiguration), statConfiguration.Properties[Environment.ConnectionString]);
			}
			else
			{
				statFactory = null;
			}
			return new DataSource(appFactory, statFactory, authenticationSettings);
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
			appCfg.AddAuxiliaryDatabaseObject(new SqlServerProgrammabilityAuxiliary());
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

		private static AuthenticationSettings createAuthenticationSettings(XElement rootElement)
		{
			var authenticationSettings = new AuthenticationSettings();

			int count = rootElement.Elements("authentication").Count();
			if (count == 1)
			{
				XElement authenticationElement = rootElement.Element("authentication");
				XElement logonMode = authenticationElement.Element("logonMode");
				string logonModeStringValue = string.Empty;
				if (logonMode == null || string.IsNullOrEmpty(logonMode.Value))
					return createDefaultAuthenticationSettings();
				logonModeStringValue = logonMode.Value;
				var logonModeValue = (LogOnModeOption)Enum.Parse(typeof(LogOnModeOption), logonModeStringValue, true);
				authenticationSettings.LogOnMode = logonModeValue;
			}
			else
			{
				return createDefaultAuthenticationSettings();
			}
			return authenticationSettings;
		}

		private static AuthenticationSettings createDefaultAuthenticationSettings()
		{
			return new AuthenticationSettings {LogOnMode = LogOnModeOption.Mix};
		}

		private Configuration createStatisticConfiguration(string connectionString, string matrixname)
		{
			//REMOVE ME LATER!!!!!!!!!!!!!!!!/((
			Log4NetConfiguration.SetConnectionString(connectionString);
			////////////////////////////////////
			using (PerformanceOutput.ForOperation("Configuring statistic db"))
			{
				var statCfg = new Configuration()
						 .SetProperty(Environment.ConnectionString, connectionString)
						 .SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider")
						 .SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver")
						 .SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect")
						 .SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName)
						 .SetProperty(Environment.SessionFactoryName, matrixname);
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
