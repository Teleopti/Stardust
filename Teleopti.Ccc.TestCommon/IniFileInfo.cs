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

		public static string SQL_AUTH_STRING { get; private set; }
		public static string DB_ANALYTICS { get; private set; }
		public static string WEB_BROKER_BACKPLANE { get; private set; }
		public static string WEB_BROKER { get; private set; }
		public static string ConnectionString { get; private set; }
		public static string ConnectionStringMatrix { get; private set; }
		public static string SitePath { get; private set; }
		public static bool SqlOutput { get; private set; }
		public static string SQL_SERVER_NAME { get; private set; }
		public static string DB_CCC7 { get; private set; }
		public static string SQL_LOGIN { get; private set; }
		public static string SQL_PASSWORD { get; private set; }
		public static bool Create { get; private set; }
		public static bool CreateByNHib { get; private set; }
		public static string Url { get; private set; }
		public static string AGENTPORTALWEB_nhibConfPath { get; private set; }
		public static bool IISExpress { get; private set; }
		public static bool ServiceBus { get; private set; }

		private static void readIniFile()
		{
			const string testDatabaseSection = "TestDatabase";
			var iniFileHelper = new IniFileHelper(ConfigurationManager.AppSettings["dbFile"]);

			DB_ANALYTICS = iniFileHelper.ReadIniValue(testDatabaseSection, "DB_ANALYTICS");
			DB_CCC7 = iniFileHelper.ReadIniValue(testDatabaseSection, "DB_CCC7");
			SQL_SERVER_NAME = iniFileHelper.ReadIniValue(testDatabaseSection, "SQL_SERVER_NAME");
			SQL_LOGIN = iniFileHelper.ReadIniValue(testDatabaseSection, "SQL_LOGIN");
			SQL_PASSWORD = iniFileHelper.ReadIniValue(testDatabaseSection, "SQL_PASSWORD");
			WEB_BROKER_BACKPLANE = iniFileHelper.ReadIniValue(testDatabaseSection, "WEB_BROKER_BACKPLANE");
			WEB_BROKER = iniFileHelper.ReadIniValue(testDatabaseSection, "WEB_BROKER");
			Create = iniFileHelper.ReadIniValue(testDatabaseSection, "create") != "false";
			CreateByNHib = iniFileHelper.ReadIniValue(testDatabaseSection, "createmode") == "nhib";
			SqlOutput = iniFileHelper.ReadIniValue(testDatabaseSection, "sqloutput") == "true";
			Url = iniFileHelper.ReadIniValue(testDatabaseSection, "url");
			AGENTPORTALWEB_nhibConfPath = iniFileHelper.ReadIniValue(testDatabaseSection, "AGENTPORTALWEB_nhibConfPath");			
			SitePath = iniFileHelper.ReadIniValue(testDatabaseSection, "sitepath");
			var iisexpress = iniFileHelper.ReadIniValue(testDatabaseSection, "iisexpress");
			IISExpress = iisexpress == "true" || string.IsNullOrEmpty(iisexpress);
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
							  SQL_SERVER_NAME, DB_CCC7, SQL_LOGIN, SQL_PASSWORD);

			ConnectionStringMatrix =
								string.Format(CultureInfo.InvariantCulture,
							  "Data Source={0};Initial Catalog={1};User Id={2};Password={3}",
							  SQL_SERVER_NAME, DB_ANALYTICS, SQL_LOGIN, SQL_PASSWORD);

			SQL_AUTH_STRING =
				string.Format(CultureInfo.InvariantCulture,
							  "Data Source={0};User Id={1};Password={2}",
							  SQL_SERVER_NAME, SQL_LOGIN, SQL_PASSWORD);


		}
	}
}