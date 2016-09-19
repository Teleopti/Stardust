using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class ForecastedStaffingViewModelCreatorTest : ISetup
	{
		public ForecastedStaffingViewModelCreator Target;
		public ForecastedStaffingProvider ForecastedStaffingProvider;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		private int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldReturnStaffingForTwoIntervals()
		{
			TimeZone.IsSweden();
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), 0);
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(new List<SkillWorkload>());

			var vm = Target.Load(new[] { skill.Id.Value });

			var staffingIntervals = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.First().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(staffingIntervals.Last().Period.StartDateTime, TimeZone.TimeZone()));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInUsTimeZoneWhenOpen247()
		{
			TimeZone.IsNewYork();
			var userNow = new DateTime(2016, 8, 26, 23, 0, 0, DateTimeKind.Local);
			var userNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(userNow.Date, TimeZone.TimeZone());
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 22, 30, 0, DateTimeKind.Local);
			var latestStatsTimeUtc = TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone());
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var skillDay1 = createSkillDay(skill, scenario, userNow.AddDays(-1), 96);
			var skillDay2 = createSkillDay(skill, scenario, userNow, 96);
			var skillDay3 = createSkillDay(skill, scenario, userNow.AddDays(1), 96);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillDayRepository.Add(skillDay3);

			var taskPeriodList = new List<ITemplateTaskPeriod>();
			taskPeriodList.AddRange(skillDay1.WorkloadDayCollection.First().TaskPeriodList
				.Where(t => t.Period.StartDateTime >= userNowStartOfDayUtc && t.Period.EndDateTime <= latestStatsTimeUtc.AddMinutes(minutesPerInterval)));
			taskPeriodList.AddRange(skillDay2.WorkloadDayCollection.First().TaskPeriodList
				.Where(t => t.Period.StartDateTime >= userNowStartOfDayUtc && t.Period.EndDateTime <= latestStatsTimeUtc.AddMinutes(minutesPerInterval)));
			taskPeriodList.AddRange(skillDay3.WorkloadDayCollection.First().TaskPeriodList
				.Where(t => t.Period.StartDateTime >= userNowStartOfDayUtc && t.Period.EndDateTime <= latestStatsTimeUtc.AddMinutes(minutesPerInterval)));

			double deviationFactor = 2;
			IntradayQueueStatisticsLoader.Has(getActualWorkloadPerSkillAndInterval(taskPeriodList, skill, deviationFactor, latestStatsTimeUtc));
			
			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow.Date);
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow.Date.AddDays(1).AddMinutes(-minutesPerInterval));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * deviationFactor);
		}
		
		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInAustraliaTimeZoneWhenOpen247()
		{
			var userNow = new DateTime(2016, 8, 26, 0, 0, 0, DateTimeKind.Local);
			TimeZone.IsAustralia();
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var skillDay1 = createSkillDay(skill, scenario, userNow.AddDays(-1), 0);
			var skillDay2 = createSkillDay(skill, scenario, userNow, 0);
			var skillDay3 = createSkillDay(skill, scenario, userNow.AddDays(1), 0);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillDayRepository.Add(skillDay3);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(96);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow);
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow.AddDays(1).AddMinutes(-minutesPerInterval));
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
		}
		
		[Test]
		public void ShouldReturnUpdatedStaffingForecastFromNowUntilEndOfDay()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, 1);
			SkillDayRepository.Add(skillDay);
			
			IntradayQueueStatisticsLoader.Has(getActualWorkloadPerSkillAndInterval(skillDay.WorkloadDayCollection.First().TaskPeriodList.ToList(), skill, 1.2d, latestStatsTime));

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.Time.First().Should().Be.EqualTo(userNow.AddMinutes(-15));
			vm.DataSeries.Time.Last().Should().Be.EqualTo(userNow);
			
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * 1.2;

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldReturnEmptyDataSeriesWhenNoForecastOnSkill()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(0);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldHandleSkill30MinutesIntervalLengthAndTarget15()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), 1);
			SkillDayRepository.Add(skillDay);

			var forecastedTask = skillDay.WorkloadDayCollection.First().TaskPeriodList
				.Single(t => t.Period.StartDateTime == latestStatsTime);
			IntradayQueueStatisticsLoader.Has(new List<SkillWorkload>
			{
				new SkillWorkload
				{
					SkillId = skill.Id.Value,
					StartTime = forecastedTask.Period.StartDateTime,
					WorkloadInSeconds =
						((forecastedTask.TotalTasks/2*
						  (forecastedTask.AverageTaskTime.TotalSeconds + forecastedTask.AverageAfterTaskTime.TotalSeconds))*1.2)
				}
				// 20% more workload than forecasted
			});

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * 1.2);
		}

		[Test]
		public void ShouldSummariseForecastedStaffingWhenMoreThanOneSkill()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);

			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), 0);
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), 0);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			var staffingIntervals1 = skillDay1.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
			var staffingIntervals2 = skillDay2.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

			vm.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(staffingIntervals1.First().FStaff + staffingIntervals2.First().FStaff);
			vm.DataSeries.ForecastedStaffing.Last().Should().Be.EqualTo(staffingIntervals1.Last().FStaff + staffingIntervals2.Last().FStaff);
		}

		[Test]
		public void ShouldShowUpdatedStaffingFromLatestStatsTimeWhenNowIsEarlier()
		{
			// This test is for dev, test and demo cases

			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);

			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, 2);
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(new List<SkillWorkload>
			{
				new SkillWorkload { SkillId = skill.Id.Value, StartTime = latestStatsTime, WorkloadInSeconds = 30 }
			});

			var vm = Target.Load(new[] { skill.Id.Value });
			
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldNotShowUpdatedStaffingWhenNoStatistics()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);

			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, 0);
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldCalculateReforecastedStaffingCorrectlyForSkillArea()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);

			var skillWithReforecast = createSkill(minutesPerInterval, "skill with reforecast", new TimePeriod(7,45,8,15));
			SkillRepository.Has(skillWithReforecast);
			
			var skillWithoutReforecast = createSkill(minutesPerInterval, "skill without reforecast", new TimePeriod(8, 0, 8, 15));
			SkillRepository.Has(skillWithoutReforecast);

			var skillDayWithReforecast = createSkillDay(skillWithReforecast, scenario, Now.UtcDateTime(), 1);
			SkillDayRepository.Add(skillDayWithReforecast);
			var skillDayWithoutReforecast = createSkillDay(skillWithoutReforecast, scenario, Now.UtcDateTime(), 1);
			SkillDayRepository.Add(skillDayWithoutReforecast);

			var actualWorkload =
				(int)
					((skillDayWithReforecast.TotalTasks*
						(skillDayWithReforecast.AverageTaskTime.TotalSeconds + skillDayWithReforecast.AverageAfterTaskTime.TotalSeconds))*
					 2);

			IntradayQueueStatisticsLoader.Has(
				new List<SkillWorkload>()
				{
					new SkillWorkload()
					{
						SkillId = skillWithReforecast.Id.Value,
						StartTime = latestStatsTime,
						WorkloadInSeconds = actualWorkload
					}
			});

			var vm = Target.Load(new[] { skillWithReforecast.Id.Value, skillWithoutReforecast.Id.Value });
			
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);

			var expectedUpdatedStaffing = skillDayWithoutReforecast.SkillStaffPeriodCollection.Last().FStaff + skillDayWithReforecast.SkillStaffPeriodCollection.Last().FStaff * 2;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedStaffing);
		}


		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}

		private ISkillDay createSkillDay(ISkill skill, IScenario scenario, DateTime userNow, int tasksForNumberOfIntervals)
		{
			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(userNow), TimeSpan.FromMinutes(60));

			for (int i = 0; i < tasksForNumberOfIntervals; i++)
			{
				skillDay.WorkloadDayCollection.First().TaskPeriodList[i].Tasks = 20;
				skillDay.WorkloadDayCollection.First().TaskPeriodList[i].AverageTaskTime = TimeSpan.FromSeconds(100);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[i].AverageAfterTaskTime = TimeSpan.FromSeconds(20);
			}

			return skillDay;
		}

		private List<SkillWorkload> getActualWorkloadPerSkillAndInterval(List<ITemplateTaskPeriod> taskPeriodList, ISkill skill, double deviationFactor, DateTime latestStatsTimeUtc)
		{
			var actualWorkloadList = new List<SkillWorkload>();
			foreach (var taskPeriod in taskPeriodList.Where(t => t.Period.StartDateTime <= latestStatsTimeUtc))
			{
				var actualWorkload = taskPeriod.Task.AverageHandlingTaskTime.TotalSeconds * taskPeriod.Task.Tasks * deviationFactor;
				actualWorkloadList.Add(new SkillWorkload
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(taskPeriod.Period.StartDateTime, TimeZone.TimeZone()),
					WorkloadInSeconds = actualWorkload
				});
			}
			return actualWorkloadList;
		}
	}
}