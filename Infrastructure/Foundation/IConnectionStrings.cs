using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IConnectionStrings
	{
		string Application();
		string Analytics();
	}
	
	public static class ConnectionStringsExtensions
	{
		public static string ApplicationFor(this IConnectionStrings instance, string applicationName)
		{
			return new SqlConnectionStringBuilder(instance.Application())
			{
				ApplicationName = applicationName
			}.ConnectionString;
		}

		public static string AnalyticsFor(this IConnectionStrings instance, string applicationName)
		{
			return new SqlConnectionStringBuilder(instance.Analytics())
			{
				ApplicationName = applicationName
			}.ConnectionString;
		}
	}
}