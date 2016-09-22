using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	// Should be merged into RaptorRepository when toggle ETL_MoveBadgeCalculationToETL_38421 removed
	public class EtlBadgeCalculationRepository : IBadgeCalculationRepository
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(EtlBadgeCalculationRepository));
		private readonly string _dataMartConnectionString;

		public EtlBadgeCalculationRepository()
		{
			_dataMartConnectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
		}

		public Dictionary<Guid, int> LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId)
		{
			var sqlParameters = new[]
			{
				new SqlParameter("threshold", answeredCallsThreshold),
				new SqlParameter("time_zone_code", timezoneCode),
				new SqlParameter("local_date", date),
				new SqlParameter("business_unit_code", businessUnitId)
			};

			var result = new Dictionary<Guid, int>();
			try
			{
				var data = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
					"[mart].[raptor_number_of_calls_per_agent_by_date]", sqlParameters, _dataMartConnectionString)
					.Tables[0];

				for (var i = 0; i < data.Rows.Count; i++)
				{
					var row = data.Rows[i];
					Guid personId;
					int answeredCalls;
					if (!Guid.TryParse(row[0].ToString(), out personId) || !int.TryParse(row[1].ToString(), out answeredCalls))
					{
						logger.ErrorFormat("[LoadAgentsOverThresholdForAnsweredCalls] Failed to convert PersonId=\"{0}\" and "
										   + "AnsweredCalls=\"{1}\" to correct value.", row[0], row[1]);
						continue;
					}
					result[personId] = answeredCalls;
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred on load agents over threshold for answered calls with threshold = "
							 + $"{answeredCallsThreshold}, time_zone_code = {timezoneCode}, local_date = {date:yyyy-MM-dd}, "
							 + $"business_unit_code = {businessUnitId}.", ex);
				return result;
			}

			return result;
		}

		public Dictionary<Guid, double> LoadAgentsOverThresholdForAdherence(
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod, string timezoneCode,
			DateTime date, Percent adherenceThreshold, Guid businessUnitId)
		{
			var reportSetting = new AdherenceReportSetting
			{
				CalculationMethod = adherenceCalculationMethod
			};

			var sqlParameters = new []
			{
				new SqlParameter("threshold", adherenceThreshold.Value),
				new SqlParameter("time_zone_code", timezoneCode),
				new SqlParameter("local_date", date),
				new SqlParameter("adherence_id", reportSetting.AdherenceIdForReport()),
				new SqlParameter("business_unit_code", businessUnitId)
			};

			var result = new Dictionary<Guid, double>();
			try
			{
				var data = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
					"[mart].[raptor_adherence_per_agent_by_date]", sqlParameters, _dataMartConnectionString)
					.Tables[0];
				for (var i = 0; i < data.Rows.Count; i++)
				{
					var row = data.Rows[i];
					Guid personId;
					double adherence;
					if (!Guid.TryParse(row[0].ToString(), out personId) || !double.TryParse(row[1].ToString(), out adherence))
					{
						logger.ErrorFormat("[LoadAgentsOverThresholdForAdherence] Failed to convert PersonId=\"{0}\" and "
										   + "Adherence=\"{1}\" to correct value.", row[0], row[1]);
						continue;
					}
					result[personId] = adherence;
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred on load agents over threshold for adherence with threshold = "
							 + $"{adherenceThreshold.Value}, time_zone_code = {timezoneCode}, "
							 + $"local_date = {date:yyyy-MM-dd}, adherence_id = {reportSetting.AdherenceIdForReport()}, "
							 + $"business_unit_code = {businessUnitId}.", ex);
				return result;
			}

			return result;
		}

		public Dictionary<Guid, double> LoadAgentsUnderThresholdForAht(string timezoneCode, DateTime date,
			TimeSpan ahtThreshold, Guid businessUnitId)
		{
			var sqlParameters = new[]
			{
				new SqlParameter("threshold", ahtThreshold.TotalSeconds),
				new SqlParameter("time_zone_code", timezoneCode),
				new SqlParameter("local_date", date),
				new SqlParameter("business_unit_code", businessUnitId)
			};

			var result = new Dictionary<Guid, double>();
			try
			{
				var data = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
					"[mart].[raptor_AHT_per_agent_by_date]", sqlParameters, _dataMartConnectionString)
					.Tables[0];
				for (var i = 0; i < data.Rows.Count; i++)
				{
					var row = data.Rows[i];
					Guid personId;
					double aht;
					if (!Guid.TryParse(row[0].ToString(), out personId) || !double.TryParse(row[1].ToString(), out aht))
					{
						logger.ErrorFormat("[LoadAgentsUnderThresholdForAht] Failed to convert PersonId=\"{0}\" and "
										   + "AHT=\"{1}\" to correct value.", row[0], row[1]);
						continue;
					}
					result[personId] = aht;
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred on load agents under threshold for AHT with threshold = "
							 + $"{ahtThreshold.TotalSeconds}, time_zone_code = {timezoneCode}, local_date = {date:yyyy-MM-dd}, "
							 + $"business_unit_code = {businessUnitId}.", ex);
				return result;
			}

			return result;
		}
	}
}