using System.Data.SqlClient;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.SqlAzure;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		public static IDataSourceConfigurationSetter ForTest()
		{
			return new DataSourceConfigurationSetter("unit tests", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForEtl()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.Etl", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForSdk()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.Sdk.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForServiceBus()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.ServiceBus.Host", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForWeb()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.Web", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForApi()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.Api", new ConfigReader());
		}
		public static IDataSourceConfigurationSetter ForDesktop()
		{
			return new DataSourceConfigurationSetter("Teleopti.Wfm.SmartClientPortal.Shell", new ConfigReader());
		}

		public const string NoDataSourceName = "[not set]";

		protected DataSourceConfigurationSetter(string applicationName,
			IConfigReader configReader)
		{
			ApplicationName = applicationName ?? string.Empty;
			if (!string.IsNullOrEmpty(configReader.AppConfig("latency")))
				UseLatency = true;
		}

		public string ApplicationName { get; }
		private bool UseLatency { get; }

		public void AddDefaultSettingsTo(Configuration nhConfiguration)
		{
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.DefaultSchema, "dbo");
			nhConfiguration.AddAssembly(typeof(Person).Assembly);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SqlExceptionConverter,
				typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.UseSecondLevelCache, "false");
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.ConnectionDriver,
				UseLatency
					? typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName
					: typeof(SqlAzureClientDriverWithLogRetries).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.TransactionStrategy,
				typeof(ReliableAdoNetTransactionFactory).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SessionFactoryName, NoDataSourceName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.OrderUpdates, "true");
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.OrderInserts, "true");
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.BatchVersionedData, "true");

			AddApplicationNameToConnectionString(nhConfiguration);
		}

		public void AddApplicationNameToConnectionString(Configuration nhConfiguration)
		{
			var connString = nhConfiguration.GetProperty(Environment.ConnectionString);
			var connStringObj = new SqlConnectionStringBuilder(connString);
			if (connStringObj.ToString().Contains("Application Name")) 
				return;
			connStringObj.ApplicationName = ApplicationName;
			nhConfiguration.SetProperty(Environment.ConnectionString, connStringObj.ToString());
		}
	}
}