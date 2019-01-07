using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface IBaseConfigurationRepository
	{
		IBaseConfiguration LoadBaseConfiguration(string connectionString);
		void SaveBaseConfiguration(string connectionString, IBaseConfiguration configuration);
	}

	public class BaseConfigurationRepository : IBaseConfigurationRepository
	{
		readonly ILog _logger = LogManager.GetLogger(typeof(BaseConfigurationRepository));

		private const string cultureKey = "Culture";
		private const string intervalLengthMinutesKey = "IntervalLengthMinutes";
		private const string timeZoneCodeKey = "TimeZoneCode";
		private const string runIndexMaintenanceKey = "RunIndexMaintenance";

		private const string insightsServiceBusAddressKey = "Insights.ServiceBusAddress";
		private const string insightsTopicNameKey = "Insights.TopicName";
		private const string insightsModelLocationKey = "Insights.ModelLocation";
		private const string insightsModelNameKey = "Insights.ModelName";
		private const string insightsAnalyticsDatabaseKey = "Insights.AnalyticsDatabase";
		private const string insightsAnalysisServiceKey = "Insights.AnalysisService";
		private const string insightsLocationKey = "Insights.Location";

		public IBaseConfiguration LoadBaseConfiguration(string connectionString)
		{
			var dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_configuration_get",
				null, connectionString);

			if (dataSet == null || dataSet.Tables.Count != 1)
				return new BaseConfiguration(null, null, null, false);

			int? culture = null;
			int? intervalLength = null;
			string timeZone = null;

			string insightsServiceBusAddress = null;
			string insightsTopicName = null;
			string insightsModelLocation = null;
			string insightsModelName = null;
			string insightsAnalyticsDatabase = null;
			string insightsAnalysisService = null;
			string insightsLocation = null;

			var runIndexMaintenance = false;

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				var key = (string)row["key"];
				var value = (string)row["value"];
				switch (key)
				{
					case cultureKey:
						culture = int.Parse(value, CultureInfo.CurrentCulture);
						break;
					case intervalLengthMinutesKey:
						intervalLength = int.Parse(value, CultureInfo.CurrentCulture);
						break;
					case timeZoneCodeKey:
						timeZone = value;
						break;
					case runIndexMaintenanceKey:
						runIndexMaintenance = bool.Parse(value);
						break;
					case insightsServiceBusAddressKey:
						insightsServiceBusAddress = value;
						break;
					case insightsTopicNameKey:
						insightsTopicName = value;
						break;
					case insightsModelLocationKey:
						insightsModelLocation = value;
						break;
					case insightsModelNameKey:
						insightsModelName = value;
						break;
					case insightsAnalyticsDatabaseKey:
						insightsAnalyticsDatabase = value;
						break;
					case insightsAnalysisServiceKey:
						insightsAnalysisService = value;
						break;
					case insightsLocationKey:
						insightsLocation = value;
						break;

					default:
						_logger.InfoFormat(CultureInfo.InvariantCulture,
							"Trying to load un unknown configuration key named: '{0}'.", key);
						break;
				}
			}

			var config = new BaseConfiguration(culture, intervalLength, timeZone, runIndexMaintenance)
			{
				InsightsConfig =
				{
					ServiceBusAddress = insightsServiceBusAddress,
					TopicName = insightsTopicName,
					AnalysisService = insightsAnalysisService,
					AnalyticsDatabase = insightsAnalyticsDatabase,
					ModelLocation = insightsModelLocation,
					ModelName = insightsModelName,
					Location = insightsLocation
				}
			};

			return config;
		}

		public void SaveBaseConfiguration(string connectionString, IBaseConfiguration configuration)
		{
			var parameterList = new[]
			{
				new SqlParameter("key", cultureKey),
				new SqlParameter("value", configuration.CultureId)
			};
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList,
				connectionString);

			parameterList = new[]
			{
				new SqlParameter("key", intervalLengthMinutesKey),
				new SqlParameter("value", configuration.IntervalLength)
			};
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList,
				connectionString);

			parameterList = new[]
			{
				new SqlParameter("key", timeZoneCodeKey),
				new SqlParameter("value", configuration.TimeZoneCode)
			};
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList,
				connectionString);
		}
	}
}