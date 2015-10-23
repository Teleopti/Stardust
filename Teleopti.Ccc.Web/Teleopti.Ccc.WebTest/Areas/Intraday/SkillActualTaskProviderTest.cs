using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Intraday
{
	public class SkillActualTaskProviderTest
	{
		[Test]
		public void ShouldReturnCorrectNumberOfSkills()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			IStatisticRepository fakeStatisticRepository = new FakeStatisticRepository();
			var target = new SkillActualTasksProvider(skillRepository, fakeStatisticRepository);
			var result = target.GetActualTasks();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnActualTaskForOneSkill()
		{
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);

			FakeStatisticRepository fakeStatisticRepository = new FakeStatisticRepository();
			IQueueSource queueSource  = new QueueSource("a","a",1);
			IStatisticTask statisticTask1 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 00, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 10
			};
			IStatisticTask statisticTask2 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 15, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 15
			};
			var statisticTaskList = new List<IStatisticTask>() {statisticTask1, statisticTask2};
			fakeStatisticRepository.FakeStatisticData(queueSource,statisticTaskList);
			var target = new SkillActualTasksProvider(skillRepository, fakeStatisticRepository);
			var result = target.GetActualTasks();
			result.Count.Should().Be.EqualTo(2);
			result.First().Value.First().Task.Should().Be.EqualTo(10);
		}
	}

	public class FakeStatisticRepository : IStatisticRepository
	{
		private Dictionary<IQueueSource,IList<IStatisticTask>> _statisticTaskDataPerQueueSource  =new Dictionary<IQueueSource, IList<IStatisticTask>>();

		public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period)
		{
			throw new NotImplementedException();
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

		public void FakeStatisticData(IQueueSource queueSource, List<IStatisticTask> statisticTaskList)
		{
			_statisticTaskDataPerQueueSource.Add(queueSource, statisticTaskList);
		}
	}

	public class SkillActualTasksProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IStatisticRepository _statisticRepository;

		public SkillActualTasksProvider(ISkillRepository skillRepository, IStatisticRepository statisticRepository)
		{
			_skillRepository = skillRepository;
			_statisticRepository = statisticRepository;
		}

		public Dictionary<ISkill,IList<SkillTaskDetails>> GetActualTasks()
		{
			var result = new Dictionary<ISkill, IList<SkillTaskDetails>>();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var queuSourceSllection = new List<IQueueSource>();
			foreach (var skill in skills)
			{
				foreach (var workload in skill.WorkloadCollection)
				{
					queuSourceSllection.AddRange(workload.QueueSourceCollection);
					//need to refactor this
					result.Add(skill, new List<SkillTaskDetails>());
					var statisticTasks = _statisticRepository.LoadSpecificDates(queuSourceSllection, new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow));
					foreach (var statisticTask in statisticTasks)
					{
						//check the resolution
						result[skill].Add(new SkillTaskDetails()
						{
							Interval = new DateTimePeriod(statisticTask.Interval, statisticTask.Interval.AddMinutes(skill.DefaultResolution))
						});
					}
				}
				
			}
			return result;
		}
	}
}
