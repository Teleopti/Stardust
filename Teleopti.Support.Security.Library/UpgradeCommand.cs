using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Security.Library
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
						command.Server = switchValue;
						break;
					case "-AP": // Application db database name
						command.ApplicationDatabase = switchValue;
						break;
					case "-AN": // Analytics db database name
						command.AnalyticsDatabase = switchValue;
						break;
					case "-CD": // Agg Db Name
						command.AggDatabase = switchValue;
						break;
					case "-DU": // Destination User Name.
						command.UserName = switchValue;
						break;
					case "-DP": // Destination Password.
						command.Password = switchValue;
						break;
					case "-EE":
						command.UseIntegratedSecurity = true;
						break;
					case "-CS": //Used by WISE to set conn string, _without_ initial catalog
						command.BaseConnectionString = switchValue;
						break;
					case "-CT":
						command.CheckTenantConnectionStrings = true;
						break;
					case "-TS":
						command.TenantStoreConnectionString = switchValue;
						break;
				}
			}

			return command;
		}

		public string Server { get; set; }
		public string ApplicationDatabase { get; set; }
		public string AnalyticsDatabase { get; set; }
		public string AggDatabase { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool UseIntegratedSecurity { get; set; }

		public string BaseConnectionString;

		public string TenantStoreConnectionString { get; set; }
		public bool CheckTenantConnectionStrings { get; set; }


		private string _applicationConnectionString;

		public string ApplicationConnectionString
		{
			get => _applicationConnectionString ?? (_applicationConnectionString = createConnectionString(null, ApplicationDatabase));
			set => _applicationConnectionString = value;
		}

		private string _applicationConnectionStringToStore;

		public string ApplicationConnectionStringToStore
		{
			get => _applicationConnectionStringToStore ?? (_applicationConnectionStringToStore = createConnectionString(BaseConnectionString, ApplicationDatabase));
			set => _applicationConnectionStringToStore = value;
		}

		private string _analyticsConnectionString;

		public string AnalyticsConnectionString
		{
			get => _analyticsConnectionString ?? (_analyticsConnectionString = createConnectionString(null, AnalyticsDatabase));
			set => _analyticsConnectionString = value;
		}

		private string _analyticsConnectionStringToStore;

		public string AnalyticsConnectionStringToStore
		{
			get => _analyticsConnectionStringToStore ?? (_analyticsConnectionStringToStore = createConnectionString(BaseConnectionString, AnalyticsDatabase));
			set => _analyticsConnectionStringToStore = value;
		}

		private string createConnectionString(string baseOnConnectionString, string database)
		{
			if (Server == null || database == null)
				return "";

			var builder = new SqlConnectionStringBuilder(baseOnConnectionString);

			if (baseOnConnectionString == null)
			{
				builder.DataSource = Server;
				builder.InitialCatalog = database;
				builder.CurrentLanguage = "us_english";

				if (UseIntegratedSecurity)
				{
					builder.IntegratedSecurity = UseIntegratedSecurity;
				}
				else
				{
					builder.UserID = UserName;
					builder.Password = Password;
				}
			}
			else
			{
				builder.InitialCatalog = database;
				builder.CurrentLanguage = "us_english";
			}

			return builder.ConnectionString;
		}
	}
}