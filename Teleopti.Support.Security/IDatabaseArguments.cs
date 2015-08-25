namespace Teleopti.Support.Security
{
	public interface IDatabaseArguments
	{
		string AggDatabase { get;  set; }

		 string AnalyticsDbConnectionString { get; set; }

		string ApplicationDbConnectionString { get; set; }

		string AnalyticsDbConnectionStringToStore { get; set; }

		string ApplicationDbConnectionStringToStore { get; set; }
	}

	public class DatabaseArguments : IDatabaseArguments
	{
		public string AggDatabase { get; set; }
		public string AnalyticsDbConnectionString { get; set; }
		public string ApplicationDbConnectionString { get; set; }
		public string AnalyticsDbConnectionStringToStore { get; set; }
		public string ApplicationDbConnectionStringToStore { get; set; }
	}
}