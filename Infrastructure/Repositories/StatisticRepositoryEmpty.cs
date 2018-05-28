using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class StatisticRepositoryEmpty : IStatisticRepository
	{
		internal StatisticRepositoryEmpty() { }

		public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources,
			DateTimePeriod period)
		{
			return new List<IStatisticTask>();
		}

		public IEnumerable<HistoricalDataDetail> GetLogObjectDetails()
		{
			throw new NotImplementedException();
		}

		public ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources,
			DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			return new List<IStatisticTask>();
		}

		public DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources)
		{
			throw new NotImplementedException();
		}

		public ICollection<IIntradayStatistics> LoadSkillStatisticForSpecificDates(DateOnly date)
		{
			throw new NotImplementedException();
		}

		public ICollection<MatrixReportInfo> LoadReports()
		{
			return null;
		}

		public ICollection<IQueueSource> LoadQueues()
		{
			return new List<IQueueSource>();
		}

		public ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period)
		{
			return new List<IActiveAgentCount>();
		}

		public void PersistFactQueues(DataTable queueDataTable) { }

		public void DeleteStgQueues() { }

		public void LoadFactQueues() { }

		public void LoadDimQueues() { }

		public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode,
			int languageId, int adherenceId)
		{
			return new List<object>();
		}

		public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId,
			Guid personCode)
		{
			return new List<object>();
		}

		public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			return new List<object>();
		}

		public IList<AgentStateReadModel> LoadActualAgentState(IEnumerable<IPerson> persons)
		{
			return new List<AgentStateReadModel>();
		}

		public AgentStateReadModel LoadOneActualAgentState(Guid value)
		{
			return new AgentStateReadModel();
		}
	}
}