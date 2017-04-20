using System.Collections.Generic;
using System.Data.SqlClient;
using NHibernate.Cfg;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DataSourceConfiguration
	{
		private readonly IDictionary<string, string> _applicationConfig;

		public DataSourceConfiguration()
		{
			ApplicationConnectionString = string.Empty;
			AnalyticsConnectionString = string.Empty;
			AggregationConnectionString = string.Empty;
			_applicationConfig = new Dictionary<string, string> { { Environment.CommandTimeout, "60" } };
		}

		public DataSourceConfiguration(string applicationConnectionString,
										string analyticsConnectionString,
										IDictionary<string, string> applicationConfig)
		{
			ApplicationConnectionString = applicationConnectionString;
			AnalyticsConnectionString = analyticsConnectionString;
			_applicationConfig = applicationConfig;
		}

		public virtual string AnalyticsConnectionString { get; protected set; }
		public virtual string ApplicationConnectionString { get; protected set; }
		public virtual string AggregationConnectionString { get; protected set; }

		public virtual IDictionary<string, string> ApplicationConfig
		{
			get { return new Dictionary<string, string>(_applicationConfig); }
		}

		public virtual string GetApplicationConfig(TenantApplicationConfigKey key)
		{
			if (!this.ApplicationConfig.ContainsKey(key.ToString()))
			{
				return string.Empty;
			}
			return ApplicationConfig[key.ToString()];
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
		public void SetApplicationConfig(string key, string value)
		{
			_applicationConfig[key] = value;
		}
	}
}