using System.Data.SqlClient;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;
using Teleopti.Wfm.Adherence.Domain.Service;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class DataSourceApplicationName
	{
		public string Name;

		public static string ForTest() => "unit tests";
		public static string ForEtl() => "Teleopti.Wfm.Etl";
		public static string ForSdk() => "Teleopti.Wfm.Sdk.Host";
		public static string ForServiceBus() => "Teleopti.Wfm.ServiceBus.Host";
		public static string ForWeb() => "Teleopti.Wfm.Web";
		public static string ForApi() => "Teleopti.Wfm.Api";
		public static string ForDesktop() => "Teleopti.Wfm.SmartClientPortal.Shell";
	}

	public class DataSourceConfigurationSetter : IDataSourceConfigurationSetter
	{
		public const string NoDataSourceName = "[not set]";

		public DataSourceConfigurationSetter(DataSourceApplicationName applicationName, IConfigReader configReader)
		{
			ApplicationName = string.Empty;
			if (applicationName != null)
				ApplicationName = applicationName.Name;
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
			nhConfiguration.AddAssembly(typeof(Rta).Assembly);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.SqlExceptionConverter,
				typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.UseSecondLevelCache, "false");
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.ConnectionDriver,
				UseLatency
					? typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName
					: typeof(ResilientSql2008ClientDriver).AssemblyQualifiedName);
			nhConfiguration.SetPropertyIfNotAlreadySet(Environment.TransactionStrategy, typeof(ResilientAdoNetTransactionFactory).AssemblyQualifiedName);
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