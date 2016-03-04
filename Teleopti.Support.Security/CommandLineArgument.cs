using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Security
{
	public class CommandLineArgument
	{
		private string _destinationServer;
		private string _destinationUserName;
		private string _destinationPassword;
		private bool _useIntegratedSecurity;
		private string _destinationAppDbDatabase;
		private string _destinationAnalDbDatabase;
		private string _baseConnstring;

		public CommandLineArgument(string[] argumentCollection)
		{
			readArguments(argumentCollection);
		}

		public IDatabaseArguments GetDatabaseArguments()
		{
			return new DatabaseArguments
			{
				AggDatabase = AggDatabase,
				AnalyticsDbConnectionString = AnalyticsDbConnectionString(),
				AnalyticsDbConnectionStringToStore = AnalyticsDbConnectionStringToStore(),
				ApplicationDbConnectionString = ApplicationDbConnectionString(),
				ApplicationDbConnectionStringToStore = ApplicationDbConnectionStringToStore()
			};
		}

		public string AggDatabase { get; private set; }

		public string AnalyticsDbConnectionString()
		{
			return createConnectionString(_destinationAnalDbDatabase);
		}

		public string ApplicationDbConnectionString()
		{
			return createConnectionString(_destinationAppDbDatabase);
		}

		public string AnalyticsDbConnectionStringToStore()
		{
			return _baseConnstring == null
				? createConnectionString(_destinationAnalDbDatabase)
				: createConnectionStringBasedOnBaseConnstring(_destinationAnalDbDatabase);
		}

		public string ApplicationDbConnectionStringToStore()
		{
			return _baseConnstring == null
				? createConnectionString(_destinationAppDbDatabase)
				: createConnectionStringBasedOnBaseConnstring(_destinationAppDbDatabase);
		}

		private string createConnectionStringBasedOnBaseConnstring(string initialCatalog)
		{
			return new SqlConnectionStringBuilder(_baseConnstring)
			{
				InitialCatalog = initialCatalog,
				CurrentLanguage = "us_english"
			}.ConnectionString;
		}

		private string createConnectionString(string initialCatalog)
		{
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

		private void readArguments(string[] argumentCollection)
		{
			foreach (string s in argumentCollection)
			{
				string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
				string switchValue = s.Remove(0, 3);

				switch (switchType)
				{
					case "-DS": // Destination Server Name.
						_destinationServer = switchValue;
						break;
					case "-AP": // Application db database name
						_destinationAppDbDatabase = switchValue;
						break;
					case "-AN": // Analytics db database name
						_destinationAnalDbDatabase = switchValue;
						break;
					case "-DU": // Destination User Name.
						_destinationUserName = switchValue;
						break;
					case "-DP": // Destination Password.
						_destinationPassword = switchValue;
						break;
					case "-EE":
						_useIntegratedSecurity = true;
						break;
					case "-CD": // Cross Db Name
						AggDatabase = switchValue;
						break;
					case "-CS": //Used by WISE to set conn string, _without_ initial catalog
						_baseConnstring = switchValue;
						break;
				}
			}
		}
	}
}