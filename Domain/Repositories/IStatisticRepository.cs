using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

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

		IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId,
			int adherenceId);

		IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);

		IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);

		IEnumerable<HistoricalDataDetail> GetLogObjectDetails();

		ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources,
			DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset);

		DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources);
	}
}