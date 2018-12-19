using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DataSourceConfiguration
	{
		public DataSourceConfiguration()
		{
			ApplicationConnectionString = string.Empty;
			AnalyticsConnectionString = string.Empty;
			AggregationConnectionString = string.Empty;
		
		}

		public DataSourceConfiguration(string applicationConnectionString,
										string analyticsConnectionString)
		{
			ApplicationConnectionString = applicationConnectionString;
			AnalyticsConnectionString = analyticsConnectionString;
			
		}

		public virtual string AnalyticsConnectionString { get; protected set; }
		public virtual string ApplicationConnectionString { get; protected set; }
		public virtual string AggregationConnectionString { get; protected set; }

		

		

		public virtual void SetApplicationConnectionString(string applicationConnectionString)
		{
			new SqlConnectionStringBuilder(applicationConnectionString);
			ApplicationConnectionString = applicationConnectionString;
		}

		public virtual void SetAnalyticsConnectionString(string analyticsConnectionString)
		{
			new SqlConnectionStringBuilder(analyticsConnectionString);
			AnalyticsConnectionString = analyticsConnectionString;
		}

		public virtual void SetAggregationConnectionString(string aggregationConnectionString)
		{
			new SqlConnectionStringBuilder(aggregationConnectionString);
			AggregationConnectionString = aggregationConnectionString;
		}
		
	}
}