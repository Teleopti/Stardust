using System.Configuration;

namespace Teleopti.Ccc.TestCommon
{
	public static class InfraTestConfigReader
	{
		public static string SQL_AUTH_STRING { get { return ConfigurationManager.AppSettings["SQL_AUTH_STRING"]; } }
		public static string DB_ANALYTICS { get { return ConfigurationManager.AppSettings["DB_ANALYTICS"]; } }
		public static string WEB_BROKER_BACKPLANE { get { return ConfigurationManager.AppSettings["WEB_BROKER_BACKPLANE"]; } }
		public static string SQL_SERVER_NAME { get { return ConfigurationManager.AppSettings["SQL_SERVER_NAME"]; } }
		public static string DB_CCC7 { get { return ConfigurationManager.AppSettings["DB_CCC7"]; } }
		public static string TOGGLE_MODE { get { return ConfigurationManager.AppSettings["TOGGLE_MODE"]; } }
	}
}
