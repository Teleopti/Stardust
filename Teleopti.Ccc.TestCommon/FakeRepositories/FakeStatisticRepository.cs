using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStatisticRepository : IStatisticRepository
	{
		private Dictionary<IQueueSource, IList<IStatisticTask>> _statisticTaskDataPerQueueSource = new Dictionary<IQueueSource, IList<IStatisticTask>>();

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

		public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId,
			int adherenceId)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public IList LoadAgentsUnderThresholdForAHT(string timezoneCode, DateTime date, TimeSpan aHTThreshold, Guid businessUnitId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<RunningEtlJob> GetRunningEtlJobs()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ForecastActualDifferNotification> ForecastActualDifferNotifications()
		{
			throw new NotImplementedException();
		}

		public ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period, string timeZoneId,
			TimeSpan midnightBreakOffset)
		{
			throw new NotImplementedException();
		}

		public DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources)
		{
			throw new NotImplementedException();
		}

		public ICollection<IIntradayStatistics> LoadSkillStatisticForSpecificDates(DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			return _intradayStat;
		}

			public void FakeStatisticData(IQueueSource queueSource, List<IStatisticTask> statisticTaskList)
			{
				_statisticTaskDataPerQueueSource.Add(queueSource, statisticTaskList);
			}
		

		private readonly List<IIntradayStatistics> _intradayStat = new List<IIntradayStatistics>(); 
		public void AddIntradayStatistics(IList<IIntradayStatistics> intradayStat)
		{
			_intradayStat.AddRange(intradayStat);
      }
	}
}