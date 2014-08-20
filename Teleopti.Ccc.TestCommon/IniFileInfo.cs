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
		public static string ConnectionString { get; private set; }
		public static string ConnectionStringMatrix { get; private set; }
		public static string SitePath { get; private set; }
		public static string SQL_SERVER_NAME { get; private set; }
		public static string DB_CCC7 { get; private set; }
		public static string SQL_LOGIN { get; private set; }
		public static string SQL_PASSWORD { get; private set; }
		public static string AGENTPORTALWEB_nhibConfPath { get; private set; }
		public static string TOGGLE_MODE { get; private set; }

		private static void readIniFile()
		{
			SitePath = ConfigurationManager.AppSettings["SitePath"];
			DB_ANALYTICS = ConfigurationManager.AppSettings["DB_ANALYTICS"];
			DB_CCC7 = ConfigurationManager.AppSettings["DB_CCC7"];
			SQL_SERVER_NAME = ConfigurationManager.AppSettings["SQL_SERVER_NAME"];
			SQL_LOGIN = ConfigurationManager.AppSettings["SQL_LOGIN"];
			SQL_PASSWORD = ConfigurationManager.AppSettings["SQL_PASSWORD"];
			WEB_BROKER_BACKPLANE = ConfigurationManager.AppSettings["WEB_BROKER_BACKPLANE"];
			AGENTPORTALWEB_nhibConfPath = ConfigurationManager.AppSettings["AGENTPORTALWEB_nhibConfPath"];
			TOGGLE_MODE = ConfigurationManager.AppSettings["TOGGLEMODE"];
			buildConnectionString();
		}

		private static void buildConnectionString()
		{
			if (String.IsNullOrEmpty(SQL_LOGIN) || String.IsNullOrEmpty(SQL_PASSWORD))
			{
				SQL_AUTH_STRING =
					string.Format(CultureInfo.InvariantCulture,
					"Data Source={0};Integrated Security=SSPI",
					SQL_SERVER_NAME);
			}
			else
			{
				SQL_AUTH_STRING =
					string.Format(CultureInfo.InvariantCulture,
					"Data Source={0};User Id={1};Password={2}",
					SQL_SERVER_NAME, SQL_LOGIN, SQL_PASSWORD);
			}
			ConnectionString = SQL_AUTH_STRING + string.Format(CultureInfo.InvariantCulture, ";Initial Catalog={0}",DB_CCC7);
			ConnectionStringMatrix = SQL_AUTH_STRING + string.Format(CultureInfo.InvariantCulture, ";Initial Catalog={0}", DB_ANALYTICS);
		}
	}
}
