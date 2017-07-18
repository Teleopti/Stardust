using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for statistic when no matrix db is defined
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-29
	/// </remarks>
	public class StatisticRepositoryEmpty : IStatisticRepository
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StatisticRepositoryEmpty"/> class.
		/// Only to be created from the factory.
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-29
		/// </remarks>
		internal StatisticRepositoryEmpty() { }

		/// <summary>
		/// Loads the specific dates.
		/// </summary>
		/// <param name="sources"></param>
		/// <param name="period"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Stub for loading matrix reports.
		/// Note: It will return null, instead of an empty list.
		/// </summary>
		/// <returns></returns>
		public ICollection<MatrixReportInfo> LoadReports()
		{
			return null;
		}

		/// <summary>
		/// Stub for loading the queues from Matrix.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-07
		/// </remarks>
		public ICollection<IQueueSource> LoadQueues()
		{
			return new List<IQueueSource>();
		}

		/// <summary>
		/// Loads the active agent count.
		/// </summary>
		/// <param name="skill">The skill.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-19
		/// </remarks>
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

		#region IStatisticRepository Members

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

		public IList<AgentStateReadModel> LoadLastAgentState(IEnumerable<Guid> personGuids)
		{
			return new List<AgentStateReadModel>();
		}

		public AgentStateReadModel LoadOneActualAgentState(Guid value)
		{
			return new AgentStateReadModel();
		}

		public void AddOrUpdateActualAgentState(AgentStateReadModel agentStateReadModel) { }

		public IList LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId, int? timeoutInSecond)
		{
			return new List<Guid>();
		}

		public IList LoadAgentsOverThresholdForAdherence(
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod, string timezoneCode, DateTime date,
			Percent adherenceThreshold, Guid businessUnitId, int? timeoutInSecond)
		{
			return new List<Guid>();
		}

		public IList LoadAgentsUnderThresholdForAHT(string timezoneCode, DateTime date, TimeSpan ahtThreshold,
			Guid businessUnitId, int? timeoutInSecond)
		{
			return new List<Guid>();
		}

		public ICollection<Guid> PersonIdsWithExternalLogOn(Guid businessUnitId)
		{
			return new List<Guid>();
		}

		#endregion IStatisticRepository Members
	}
}