using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BadgeCalculationRepository : IBadgeCalculationRepository
	{
		private const int timeoutInSecond = 2400;
		private readonly ILog _logger = LogManager.GetLogger(typeof(BadgeCalculationRepository));

		public Dictionary<Guid, int> LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId)
		{
			return repositoryActionWithRetry(uow =>
			{
				const string sql = "exec [mart].[raptor_number_of_calls_per_agent_by_date] @threshold=:threshold, "
								   + "@time_zone_code=:timezoneCode, @local_date=:date, @business_unit_code=:businessUnitId";

				try
				{
					return uow.Session().CreateSQLQuery(sql)
						.SetTimeout(timeoutInSecond)
						.SetReadOnly(true)
						.SetInt32("threshold", answeredCallsThreshold)
						.SetString("timezoneCode", timezoneCode)
						.SetDateTime("date", date)
						.SetGuid("businessUnitId", businessUnitId)
						.List().Cast<object[]>()
						.ToDictionary(data => (Guid) data[0], data => int.Parse(data[1].ToString()));
				}
				catch (Exception)
				{
					_logger.Error("Error occurred on load agents over threshold for answered calls with threshold = "
								  + $"{answeredCallsThreshold}, time_zone_code = {timezoneCode}, local_date = {date:yyyy-MM-dd}, "
								  + $"business_unit_code = {businessUnitId} (Timeout={timeoutInSecond}).");
					throw;
				}
			});
		}

		public Dictionary<Guid, double> LoadAgentsOverThresholdForAdherence(
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId)
		{
			var reportSetting = new AdherenceReportSetting
			{
				CalculationMethod = adherenceCalculationMethod
			};

			return repositoryActionWithRetry(uow =>
			{
				const string sql = "exec [mart].[raptor_adherence_per_agent_by_date] @threshold=:threshold, "
								   + "@time_zone_code=:timezoneCode, @local_date=:date, @adherence_id=:adherenceId, "
								   + "@business_unit_code=:businessUnitId";

				try
				{
					return uow.Session().CreateSQLQuery(sql)
						.SetTimeout(timeoutInSecond)
						.SetReadOnly(true)
						.SetDouble("threshold", adherenceThreshold.Value)
						.SetString("timezoneCode", timezoneCode)
						.SetDateTime("date", date)
						.SetInt32("adherenceId", reportSetting.AdherenceIdForReport())
						.SetGuid("businessUnitId", businessUnitId)
						.List().Cast<object[]>()
						.ToDictionary(data => (Guid) data[0], data => double.Parse(data[1].ToString()));
				}
				catch (Exception)
				{
					_logger.Error("Error occurred on load agents over threshold for adherence with threshold = "
								  + $"{adherenceThreshold.Value}, time_zone_code = {timezoneCode}, "
								  + $"local_date = {date:yyyy-MM-dd}, adherence_id = {reportSetting.AdherenceIdForReport()}, "
								  + $"business_unit_code = {businessUnitId} (Timeout={timeoutInSecond}).");
					throw;
				}
			});
		}

		public Dictionary<Guid, double> LoadAgentsUnderThresholdForAht(string timezoneCode, DateTime date,
			TimeSpan ahtThreshold,
			Guid businessUnitId)
		{
			return repositoryActionWithRetry(uow =>
			{
				const string sql = "exec [mart].[raptor_AHT_per_agent_by_date] @threshold=:threshold, "
								   + "@time_zone_code=:timezoneCode, @local_date=:date, @business_unit_code=:businessUnitId";

				try
				{
					return uow.Session().CreateSQLQuery(sql)
						.SetTimeout(timeoutInSecond)
						.SetReadOnly(true)
						.SetDouble("threshold", ahtThreshold.TotalSeconds)
						.SetString("timezoneCode", timezoneCode)
						.SetDateTime("date", date)
						.SetGuid("businessUnitId", businessUnitId)
						.List().Cast<object[]>()
						.ToDictionary(data => (Guid) data[0], data => double.Parse(data[1].ToString()));
				}
				catch (Exception)
				{
					_logger.Error("Error occurred on load agents under threshold for AHT with threshold = "
								  + $"{ahtThreshold.TotalSeconds}, time_zone_code = {timezoneCode}, local_date = {date:yyyy-MM-dd}, "
								  + $"business_unit_code = {businessUnitId} (Timeout={timeoutInSecond}).");
					throw;
				}
			});
		}

		private TResult repositoryActionWithRetry<TResult>(Func<IStatelessUnitOfWork, TResult> innerAction, int attempt = 0)
		{
			try
			{
				using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
				{
					var ret = innerAction(uow);
					return ret;
				}
			}
			catch (SqlException ex)
			{
				if (!(ex.InnerException is Win32Exception) || attempt >= 6) throw;

				_logger.Warn($"Retry - Count:{attempt}", ex);
				return repositoryActionWithRetry(innerAction, ++attempt);
			}
		}

		private static IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipalForLegacy.CurrentPrincipal.Identity);
			return identity.DataSource.Analytics;
		}
	}
}