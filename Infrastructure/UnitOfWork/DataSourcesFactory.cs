﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transaction;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	/// <summary>
	/// Factory for LogonableDataSources
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-23
	/// </remarks>
	public class DataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEnversConfiguration _enversConfiguration;
		private readonly IEnumerable<IDenormalizer> _externalDenormalizers;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DataSourcesFactory));
		private Configuration _applicationConfiguration;
		private Configuration _statisticConfiguration;
		private AuthenticationSettings _authenticationSettings;

		public const string NoDataSourceName = "[not set]";
		private const string _connectionStringKeyName = "connection.connection_string";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Denormalizers")]
		public DataSourcesFactory(IEnversConfiguration enversConfiguration, IEnumerable<IDenormalizer> externalDenormalizers)
		{
			_enversConfiguration = enversConfiguration;
			_externalDenormalizers = externalDenormalizers;
			UseCache = true;
		}

		public bool UseCache { get; set; }
		public bool UseDistributedTransactionFactory { get; set; }
		public string SessionContext { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
			createApplicationConfiguration(element);
			if (_applicationConfiguration.Properties.ContainsKey(_connectionStringKeyName))
			{
				string connectionString = _applicationConfiguration.Properties[_connectionStringKeyName];
				string resultOfOnline = isSqlServerOnline(connectionString);
				if (string.IsNullOrEmpty(resultOfOnline))
				{
                    dataSource = Create(element, connectionString);
					return true;
				}
			}
			dataSource = null;
			return false;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "dataSource"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IDataSource Create(XElement hibernateConfiguration, string connectionString)
		{
            var buildedConnectionString = new SqlConnectionStringBuilder(connectionString);
            
			if (hibernateConfiguration.Name != "datasource")
				throw new DataSourceException(@"Missing <dataSource> in xml source ");
			using (PerformanceOutput.ForOperation("Create application configuration"))
				createApplicationConfiguration(hibernateConfiguration);
			using (PerformanceOutput.ForOperation("Create statistic configuration"))
				_statisticConfiguration = createStatisticConfiguration(hibernateConfiguration);
			using (PerformanceOutput.ForOperation("Create authentication settings"))
				_authenticationSettings = createAuthenticationSettings(hibernateConfiguration);
			var appFact =
				 new NHibernateUnitOfWorkFactory(buildSessionFactory(_applicationConfiguration), _enversConfiguration.AuditSettingProvider, _externalDenormalizers);
			if (_statisticConfiguration == null)
			{
                return new DataSource(appFact, null, _authenticationSettings, buildedConnectionString);

			}
			return
				 new DataSource(appFact,
                                      new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(_statisticConfiguration)), _authenticationSettings, buildedConnectionString);
		}

		public IDataSource Create(IDictionary<string, string> settings,
												  string statisticConnectionString)
		{
			NHibernateUnitOfWorkMatrixFactory statFactory;
			createApplicationConfiguration(settings);
			var appFactory = new NHibernateUnitOfWorkFactory(buildSessionFactory(_applicationConfiguration), _enversConfiguration.AuditSettingProvider,_externalDenormalizers);
			if (!string.IsNullOrEmpty(statisticConnectionString))
			{
				_statisticConfiguration = createStatisticConfigurationInner(statisticConnectionString, NoDataSourceName);
				statFactory = new NHibernateUnitOfWorkMatrixFactory(buildSessionFactory(_statisticConfiguration));
			}
			else
			{
				_statisticConfiguration = null;
				statFactory = null;
			}
			_authenticationSettings = createDefaultAuthenticationSettings();
			return new DataSource(appFactory, statFactory, _authenticationSettings);
		}

		private static ISessionFactory buildSessionFactory(Configuration nhConf)
		{
			using (PerformanceOutput.ForOperation("Building sessionfactory for " + nhConf.Properties[Environment.SessionFactoryName]))
			{
				return nhConf.BuildSessionFactory();
			}
		}

		private void createApplicationConfiguration(IDictionary<string, string> settings)
		{
			var appCfg = new Configuration();
			setDefaultValuesOnApplicationConf(appCfg);
			appCfg.SetProperties(settings);
			appCfg.AddAuxiliaryDatabaseObject(new SqlServerProgrammabilityAuxiliary());
			_applicationConfiguration = appCfg;
		}

		private void createApplicationConfiguration(XElement nhibernateConfiguration)
		{
			string temporaryConfigFile = Path.GetTempFileName();
			try
			{
				var settings = new XmlWriterSettings {Indent = true, IndentChars = ("\t"), OmitXmlDeclaration = true};
			    using (XmlWriter xmlWriter = XmlWriter.Create(temporaryConfigFile, settings))
				{
					nhibernateConfiguration.WriteTo(xmlWriter);
				}

				var appCfg = new Configuration();
				setDefaultValuesOnApplicationConf(appCfg);
				appCfg.Configure(temporaryConfigFile);
				_applicationConfiguration = appCfg;
			}
			finally
			{
				if (File.Exists(temporaryConfigFile))
					File.Delete(temporaryConfigFile);
			}
		}

		public void CreateSchema()
		{
			//Add Schema
			const string sql = "CREATE SCHEMA [Auditing] AUTHORIZATION [dbo]";
			using (var conn = new SqlConnection(_applicationConfiguration.Properties[_connectionStringKeyName]))
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
					cmd.ExecuteNonQuery();
			}

			var appSchema = new SchemaExport(_applicationConfiguration);
			appSchema.Create(false, true);
		}

		private static Configuration createStatisticConfiguration(string file, XElement rootElement)
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

		private static Configuration createStatisticConfiguration(XElement rootElement)
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

		private static Configuration createStatisticConfigurationInner(string connectionString, string matrixname)
		{
			using (PerformanceOutput.ForOperation("Configuring statistic db"))
			{
				var statCfg = new Configuration()
						 .SetProperty(Environment.ConnectionString, connectionString)
						 .SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider")
						 .SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver")
						 .SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect")
						 .SetProperty(Environment.SqlExceptionConverter, typeof(SqlServerExceptionConverter).AssemblyQualifiedName)
						 .SetProperty(Environment.SessionFactoryName, matrixname);
				return statCfg;
			}
		}

		private void setDefaultValuesOnApplicationConf(Configuration cfg)
		{
			cfg.SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			cfg.SetProperty(Environment.ConnectionProvider, typeof (TeleoptiDriverConnectionProvider).AssemblyQualifiedName);
			cfg.SetProperty(Environment.DefaultSchema, "dbo");
			cfg.SetProperty(Environment.TransactionStrategy, "NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
			cfg.SetProperty(Environment.SessionFactoryName, NoDataSourceName);
			cfg.SetNamingStrategy(TeleoptiDatabaseNamingStrategy.Instance);
			cfg.AddAssembly("Teleopti.Ccc.Domain");
			cfg.SetProperty(Environment.SqlExceptionConverter, typeof (SqlServerExceptionConverter).AssemblyQualifiedName);
			if (UseCache)
			{
				cfg.SetProperty(Environment.CacheProvider, "NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache");
				cfg.SetProperty(Environment.UseSecondLevelCache, "true");
				cfg.SetProperty(Environment.UseQueryCache, "true");
			}
			else
			{
				cfg.SetProperty(Environment.UseSecondLevelCache, "false");
			}
			if (UseDistributedTransactionFactory)
			{
				cfg.SetProperty(Environment.TransactionStrategy,
				                typeof (AdoNetWithDistributedTransactionFactory).FullName);
			}
			_enversConfiguration.Configure(cfg);
		}
	}
}
