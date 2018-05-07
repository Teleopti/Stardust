﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStatisticRepository : IStatisticRepository
	{
		private Dictionary<IQueueSource, IList<IStatisticTask>> _statisticTaskDataPerQueueSource =
			new Dictionary<IQueueSource, IList<IStatisticTask>>();

		public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period)
		{
			IList<IStatisticTask> result = new List<IStatisticTask>();
			foreach (var source in sources)
			{
				if (_statisticTaskDataPerQueueSource.ContainsKey(source))
				{
					foreach (var task in _statisticTaskDataPerQueueSource[source])
					{
						result.Add(task);
					}
				}
			}
			return result;
		}

		public ICollection<MatrixReportInfo> LoadReports()
		{
			throw new NotImplementedException();
		}

		public ICollection<IQueueSource> LoadQueues()
		{
			throw new NotImplementedException();
		}

		public ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public void PersistFactQueues(DataTable queueDataTable)
		{
			throw new NotImplementedException();
		}

		public void DeleteStgQueues()
		{
			throw new NotImplementedException();
		}

		public void LoadFactQueues()
		{
			throw new NotImplementedException();
		}

		public void LoadDimQueues()
		{
			throw new NotImplementedException();
		}

		public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode,
			int languageId,
			int adherenceId)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId,
			Guid personCode)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<HistoricalDataDetail> GetLogObjectDetails()
		{
			return new List<HistoricalDataDetail>();
		}

		public ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources,
			DateTimePeriod period, string timeZoneId,
			TimeSpan midnightBreakOffset)
		{
			throw new NotImplementedException();
		}

		public DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources)
		{
			throw new NotImplementedException();
		}

		public ICollection<IIntradayStatistics> LoadSkillStatisticForSpecificDates(DateOnly date)
		{
			return _intradayStat;
		}

		private readonly List<IIntradayStatistics> _intradayStat = new List<IIntradayStatistics>();

		public void AddIntradayStatistics(IList<IIntradayStatistics> intradayStat)
		{
			_intradayStat.AddRange(intradayStat);
		}
	}
}