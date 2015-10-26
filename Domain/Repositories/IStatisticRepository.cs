using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IStatisticRepository
	{
		ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period);

		ICollection<MatrixReportInfo> LoadReports();

		ICollection<IQueueSource> LoadQueues();

		ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period);

		void PersistFactQueues(DataTable queueDataTable);

		void DeleteStgQueues();

		void LoadFactQueues();

		void LoadDimQueues();

		IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode,
										 Guid agentPersonCode, int languageId, int adherenceId);

		IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);

		IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);

		IList LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold, Guid businessUnitId);
		IList LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod, string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId);
		IList LoadAgentsUnderThresholdForAHT(string timezoneCode, DateTime date, TimeSpan aHTThreshold, Guid businessUnitId);

		IEnumerable<RunningEtlJob> GetRunningEtlJobs();

		IEnumerable<ForecastActualDifferNotification> ForecastActualDifferNotifications();
		ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset);
		DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources);

		ICollection<IIntradayStatistics> LoadSkillStatisticForSpecificDates(DateTimePeriod period);
	}
}