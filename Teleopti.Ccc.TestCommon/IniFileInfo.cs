using System;
using System.Configuration;
using System.Globalization;

namespace Teleopti.Ccc.TestCommon
{
	public static class IniFileInfo
	{
		static IniFileInfo()
		{
			readIniFile();
		}

		public static string ConnectionString { get; private set; }
		public static string ConnectionStringMatrix { get; private set; }

		public static bool SqlOutput { get; private set; }
		public static string ServerName { get; private set; }
		public static string Database { get; private set; }
		public static string UserName { get; private set; }
		public static string Password { get; private set; }
		public static bool Create { get; private set; }
		public static bool CreateByNHib { get; private set; }
		public static string Url { get; private set; }
		public static string SitePath { get; private set; }
		public static bool CassiniDev { get; private set; }
		public static bool ServiceBus { get; private set; }

		private static void readIniFile()
		{
			const string testDatabaseSection = "TestDatabase";
			var iniFileHelper = new IniFileHelper(ConfigurationManager.AppSettings["dbFile"]);

			Database = iniFileHelper.ReadIniValue(testDatabaseSection, "dbname");
			ServerName = iniFileHelper.ReadIniValue(testDatabaseSection, "servername");
			UserName = iniFileHelper.ReadIniValue(testDatabaseSection, "user");
			Password = iniFileHelper.ReadIniValue(testDatabaseSection, "password");
			Create = iniFileHelper.ReadIniValue(testDatabaseSection, "create") != "false";
			CreateByNHib = iniFileHelper.ReadIniValue(testDatabaseSection, "createmode") == "nhib";
			SqlOutput = iniFileHelper.ReadIniValue(testDatabaseSection, "sqloutput") == "true";
			ConnectionStringMatrix = iniFileHelper.ReadIniValue(testDatabaseSection, "matrix");
			Url = iniFileHelper.ReadIniValue(testDatabaseSection, "url");
			SitePath = iniFileHelper.ReadIniValue(testDatabaseSection, "sitepath");
			CassiniDev = iniFileHelper.ReadIniValue(testDatabaseSection, "cassinidev") == "true";
			ServiceBus = iniFileHelper.ReadIniValue(testDatabaseSection, "servicebus") == "true";

			buildConnectionString();

		}

		private static void buildConnectionString()
		{
			if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
			{
							ConnectionString =
				string.Format(CultureInfo.InvariantCulture,
							  "Data Source={0};Initial Catalog={1};Integrated Security=SSPI",
				              ServerName, Database);
			}
			else {
			ConnectionString =
				string.Format(CultureInfo.InvariantCulture,
				              "Data Source={0};Initial Catalog={1};User Id={2};Password={3}",
				              ServerName, Database, UserName, Password);
			}
		}
	}
}