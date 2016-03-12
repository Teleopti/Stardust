using System.Configuration;

namespace Teleopti.Ccc.TestCommon
{
	public static class InfraTestConfigReader
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.AppSettings["InfraTest.ConnectionString"];
			}
		}

		public static string InvalidConnectionString
		{
			get
			{
				return @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi;Connect Timeout=1;";
			}
		}

		public static string AnalyticsConnectionString
		{
			get
			{
				return ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"];
			}
		}
			
		public static string SqlServerName
		{
			get
			{
				return ConfigurationManager.AppSettings["InfraTest.SqlServerName"];
			}
		}

		public static string DatabaseName
		{
			get
			{
				return ConfigurationManager.AppSettings["InfraTest.DatabaseName"];
			}
		}

		public static string DatabaseBackupLocation
		{
			get
			{
				return ConfigurationManager.AppSettings["InfraTest.DatabaseBackupLocation"];
			}
		}

	}
}