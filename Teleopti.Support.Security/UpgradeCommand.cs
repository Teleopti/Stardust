using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Security
{
	public class UpgradeCommand
	{
		public static UpgradeCommand Parse(IEnumerable<string> argumentCollection)
		{
			var command = new UpgradeCommand();

			foreach (var s in argumentCollection)
			{
				var switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
				var switchValue = s.Remove(0, 3);

				switch (switchType)
				{
					case "-DS": // Destination Server Name.
						command._destinationServer = switchValue;
						break;
					case "-AP": // Application db database name
						command._destinationAppDbDatabase = switchValue;
						break;
					case "-AN": // Analytics db database name
						command._destinationAnalDbDatabase = switchValue;
						break;
					case "-DU": // Destination User Name.
						command._destinationUserName = switchValue;
						break;
					case "-DP": // Destination Password.
						command._destinationPassword = switchValue;
						break;
					case "-EE":
						command._useIntegratedSecurity = true;
						break;
					case "-CD": // Cross Db Name
						command.AggDatabase = switchValue;
						break;
					case "-CS": //Used by WISE to set conn string, _without_ initial catalog
						command._baseConnstring = switchValue;
						break;
					case "-CT":
						command.CheckTenantConnectionStrings = true;
						break;
					case "-TS":
						command.TenantStoreConnectionString = switchValue;
						break;
				}
			}

			command.AnalyticsDbConnectionString = command.analyticsDbConnectionString();
			command.AnalyticsDbConnectionStringToStore = command.analyticsDbConnectionStringToStore();
			command.ApplicationDbConnectionString = command.applicationDbConnectionString();
			command.ApplicationDbConnectionStringToStore = command.applicationDbConnectionStringToStore();
			command.CheckTenantConnectionStrings = command.CheckTenantConnectionStrings;
			command.TenantStoreConnectionString = command.TenantStoreConnectionString;

			return command;
		}

		public string _destinationServer;
		public string _destinationUserName;
		public string _destinationPassword;
		public bool _useIntegratedSecurity;
		public string _destinationAppDbDatabase;
		public string _destinationAnalDbDatabase;
		public string _baseConnstring;

		public string AggDatabase { get; set; }
		public string AnalyticsDbConnectionString { get; set; }
		public string ApplicationDbConnectionString { get; set; }
		public string AnalyticsDbConnectionStringToStore { get; set; }
		public string ApplicationDbConnectionStringToStore { get; set; }
		public string TenantStoreConnectionString { get; set; }
		public bool CheckTenantConnectionStrings { get; set; }

		public string analyticsDbConnectionString() => createConnectionString(_destinationAnalDbDatabase);

		public string applicationDbConnectionString() => createConnectionString(_destinationAppDbDatabase);

		public string analyticsDbConnectionStringToStore() =>
			_baseConnstring == null
				? createConnectionString(_destinationAnalDbDatabase)
				: createConnectionStringBasedOnBaseConnstring(_destinationAnalDbDatabase);

		public string applicationDbConnectionStringToStore() =>
			_baseConnstring == null
				? createConnectionString(_destinationAppDbDatabase)
				: createConnectionStringBasedOnBaseConnstring(_destinationAppDbDatabase);

		private string createConnectionStringBasedOnBaseConnstring(string initialCatalog)
		{
			if (_baseConnstring == null || initialCatalog == null)
				return "";
			return new SqlConnectionStringBuilder(_baseConnstring)
			{
				InitialCatalog = initialCatalog,
				CurrentLanguage = "us_english"
			}.ConnectionString;
		}

		private string createConnectionString(string initialCatalog)
		{
			if (_destinationServer == null || initialCatalog == null)
				return "";

			var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
			{
				DataSource = _destinationServer,
				InitialCatalog = initialCatalog,
				CurrentLanguage = "us_english"
			};

			if (_useIntegratedSecurity)
			{
				sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
			}
			else
			{
				sqlConnectionStringBuilder.UserID = _destinationUserName;
				sqlConnectionStringBuilder.Password = _destinationPassword;
			}

			return sqlConnectionStringBuilder.ConnectionString;
		}
	}
}