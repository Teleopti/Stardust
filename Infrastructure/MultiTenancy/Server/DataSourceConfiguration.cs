using System.Collections.Generic;
using System.Data.SqlClient;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DataSourceConfiguration
	{
		private readonly IDictionary<string, string> _applicationNHibernateConfig;

		public DataSourceConfiguration()
		{
			ApplicationConnectionString = string.Empty;
			AnalyticsConnectionString = string.Empty;
			AggregationConnectionString = string.Empty;
			_applicationNHibernateConfig = new Dictionary<string, string> { { Environment.CommandTimeout, "60" } };
		}

		public DataSourceConfiguration(string applicationConnectionString, 
										string analyticsConnectionString,
										IDictionary<string, string> applicationNHibernateConfig)
		{
			ApplicationConnectionString = applicationConnectionString;
			AnalyticsConnectionString = analyticsConnectionString;
			_applicationNHibernateConfig = applicationNHibernateConfig;
		}

		public virtual string AnalyticsConnectionString { get; protected set; }
		public virtual string ApplicationConnectionString { get; protected set; }
		public virtual string AggregationConnectionString { get; protected set; }

		public virtual IDictionary<string, string> ApplicationNHibernateConfig
		{
			get { return new Dictionary<string, string>(_applicationNHibernateConfig); }
		}

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
		public void SetNHibernateConfig(string key, string value)
		{
			_applicationNHibernateConfig[key] = value;
		}
	}
}