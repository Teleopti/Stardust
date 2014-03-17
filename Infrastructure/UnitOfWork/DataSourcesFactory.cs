using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "dataSource")]
		public bool TryCreate(string file, out IDataSource dataSource)
		{
			XElement element = XElement.Load(file);
			if (element.Name != "datasource")
			{
				throw new DataSourceException(@"Missing <dataSource> in file " + file);
			}

			if (TryCreate(element, out dataSource))
			{
				return true;
			}

			dataSource = null;
			return false;
		}

		public bool TryCreate(XElement element, out IDataSource dataSource)
		{
			var appConfig = createApplicationConfiguration(element);
			if (appConfig.Properties.ContainsKey(Environment.ConnectionString))
			{
				string connectionString = appConfig.Properties[Environment.ConnectionString];
				string resultOfOnline = isSqlServerOnline(connectionString);
				if (string.IsNullOrEmpty(resultOfOnline))
				{
					dataSource = Create(element);
					return true;
				}
			}
			dataSource = null;
			return false;
		}

		private IDataSource Create(XElement hibernateConfiguration)
		{
			if (hibernateConfiguration.Name != "datasource")
				throw new DataSourceException(@"Missing <dataSource> in xml source ");
			var appConfig = createApplicationConfiguration(hibernateConfiguration);
			var statConfiguration = createStatisticConfiguration(hibernateConfiguration);
			var authenticationSettings = createAuthenticationSettings(hibernateConfiguration);
			var appFact = new NHibernateUnitOfWorkFactory(buildSessionFactory(appConfig), _enversConfiguration.AuditSettingProvider, appConfig.Properties[Environment.ConnectionString], _messageSenders);
			if (statConfiguration == null)
			{
				return new DataSource(appFact, null, authenticationSettings);
			}
			return
				 new DataSource(appFact, new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(statConfiguration), statConfiguration.Properties[Environment.ConnectionString]), authenticationSettings);
		}

		public IDataSource Create(IDictionary<string, string> settings,
												  string statisticConnectionString)
		{
			NHibernateUnitOfWorkMatrixFactory statFactory;
			var appConfig = createApplicationConfiguration(settings);
			var appFactory = new NHibernateUnitOfWorkFactory(buildSessionFactory(appConfig), _enversConfiguration.AuditSettingProvider,appConfig.Properties[Environment.ConnectionString],_messageSenders);
			if (!string.IsNullOrEmpty(statisticConnectionString))
			{
				var statConfiguration = createStatisticConfigurationInner(statisticConnectionString, NoDataSourceName);
				statFactory = new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(statConfiguration), statConfiguration.Properties[Environment.ConnectionString]);
			}
			else
			{
				statFactory = null;
			}
			var authenticationSettings = createDefaultAuthenticationSettings();
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
			foreach (var item in settings)
			{
				appCfg.SetProperty(item.Key, item.Value);
			}
			setDefaultValuesOnApplicationConf(appCfg);
			appCfg.AddAuxiliaryDatabaseObject(new SqlServerProgrammabilityAuxiliary());
			return appCfg;
		}

		private Configuration createApplicationConfiguration(XElement nhibernateConfiguration)
		{
			var temporaryConfigFile = Path.GetTempFileName();
			try
			{
				var settings = new XmlWriterSettings {Indent = true, IndentChars = ("\t"), OmitXmlDeclaration = true};
			    using (XmlWriter xmlWriter = XmlWriter.Create(temporaryConfigFile, settings))
				{
					nhibernateConfiguration.WriteTo(xmlWriter);
				}

				var appCfg = new Configuration();
				appCfg.Configure(temporaryConfigFile);
				setDefaultValuesOnApplicationConf(appCfg);
				return appCfg;
			}
			finally
			{
				if (File.Exists(temporaryConfigFile))
					File.Delete(temporaryConfigFile);
			}
		}

		private Configuration createStatisticConfiguration(string file, XElement rootElement)
		{
			Configuration statCfg = null;

			int count = rootElement.Elements("matrix").Count();
			if (count == 1)
			{
				XElement matrixElement = rootElement.Element("matrix");
				XElement connString = matrixElement.Element("connectionString");
				if (connString == null || string.IsNullOrEmpty(connString.Value))
				{
					if (String.IsNullOrEmpty(file))
					{
						throw new DataSourceException(@"Missing <connectionString> in xml structure");
					}
					throw new DataSourceException(@"Missing <connectionString> in file " + file);
				}

				XAttribute matrixAttribute = matrixElement.Attribute("name");
				string matrixname = NoDataSourceName;
				if (matrixAttribute != null)
					matrixname = matrixAttribute.Value;

				statCfg = createStatisticConfigurationInner(connString.Value, matrixname);
			}
			return statCfg;
		}

		private Configuration createStatisticConfiguration(XElement rootElement)
		{
			return createStatisticConfiguration(String.Empty, rootElement);
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
			var authenticationSettings = new AuthenticationSettings {LogOnMode = LogOnModeOption.Mix};

		    return authenticationSettings;
		}

		private Configuration createStatisticConfigurationInner(string connectionString, string matrixname)
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
