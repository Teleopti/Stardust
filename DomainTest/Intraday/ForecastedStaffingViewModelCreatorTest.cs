using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
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
		public void ShouldReturnForecastedStaffingForTwoIntervals()
		{
			TimeZone.IsSweden();
			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));
			SkillDayRepository.Add(skillDay);

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
		public void ShouldReturnUpdatedStaffingForecastFromNowUntilEndOfOpenHour()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30));
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(getActualCallsPerSkillAndInterval(skillDay.WorkloadDayCollection.First().TaskPeriodList.ToList(), skill, 1.2d, latestStatsTime));

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * 1.2;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldReturnUpdatedStaffingForecastCorrectlyWithMoreThanOneWorkload()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));

			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30));

			IWorkload workload = new Workload(skill);
			workload.Description = "second workload";
			workload.Name = "second name from factory";
			workload.TemplateWeekCollection.ForEach(x => x.Value.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(8, 0, 8, 30) }));
			var workloadDay = new WorkloadDay();
			workloadDay.CreateFromTemplate(new DateOnly(userNow), workload,
				(IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, userNow.DayOfWeek));
			skillDay.AddWorkloadDay(workloadDay);

			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 5;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[0].Tasks = 10;

			var actualCallsPerSkill = skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks +
									  skillDay.WorkloadDayCollection.Last().TaskPeriodList[0].Tasks;

			SkillDayRepository.Add(skillDay);

			var actualIntervals = new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc),
					Calls = actualCallsPerSkill * 1.2d
				},
			};

			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			var expectedUpdatedForecastOnLastInterval = vm.DataSeries.ForecastedStaffing.Last() * 1.2d;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedForecastOnLastInterval);
		}

		[Test]
		public void ShouldSkipZeroForecastedCallIntervalsWhenCalculatingUpdatedForecast()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var openHours = new TimePeriod(8, 0, 8, 45);

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", openHours);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 1, new Tuple<TimePeriod, double>(openHours, 1));
			skillDay.WorkloadDayCollection.First().TaskPeriodList[0].Tasks = 0.00008;
			skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks = 2;
			skillDay.WorkloadDayCollection.Last().TaskPeriodList[2].Tasks = 2;

			var actualIntervals = new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc),
					Calls = 1
				},
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc),
					Calls = 2
				}
			};

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			IntradayQueueStatisticsLoader.Has(actualIntervals);

			var vm = Target.Load(new[] { skill.Id.Value });

			var expectedDeviation = vm.DataSeries.ForecastedStaffing.First() * actualIntervals[1].Calls / skillDay.WorkloadDayCollection.First().TaskPeriodList[1].Tasks;
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * expectedDeviation);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInUsTimeZoneWhenOpen247()
		{
			TimeZone.IsNewYork();
			var userNow = new DateTime(2016, 8, 26, 23, 0, 0, DateTimeKind.Local);
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 22, 30, 0, DateTimeKind.Local);
			var latestStatsTimeUtc = TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone());
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var taskPeriodList = forecastedTasksUpUntilLatestStatsTime(skill, scenario, userNow, latestStatsTimeUtc);

			double deviationFactor = 2;
			IntradayQueueStatisticsLoader.Has(getActualCallsPerSkillAndInterval(taskPeriodList, skill, deviationFactor,
				latestStatsTimeUtc));

			var vm = Target.Load(new[] {skill.Id.Value});

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing[92].Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing[92]*deviationFactor);
			vm.DataSeries.UpdatedForecastedStaffing[91].Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnStaffingForUsersCurrentDayInAustraliaTimeZoneWhenOpen247()
		{
			TimeZone.IsAustralia();
			var userNow = new DateTime(2016, 8, 26, 2, 0, 0, DateTimeKind.Local);
			var latestStatsTimeLocal = new DateTime(2016, 8, 26, 1, 45, 0, DateTimeKind.Local);
			var latestStatsTimeUtc = TimeZoneHelper.ConvertToUtc(latestStatsTimeLocal, TimeZone.TimeZone());
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(0, 0, 24, 0));
			SkillRepository.Has(skill);

			var taskPeriodList = forecastedTasksUpUntilLatestStatsTime(skill, scenario, userNow, latestStatsTimeUtc);

			double deviationFactor = 2;
			IntradayQueueStatisticsLoader.Has(getActualCallsPerSkillAndInterval(taskPeriodList, skill, deviationFactor,
				latestStatsTimeUtc));

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(96);
			vm.DataSeries.UpdatedForecastedStaffing[7].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing[8].Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing[8] * deviationFactor);
		}
		
		[Test]
		public void ShouldReturnEmptyDataSeriesWhenNoForecastOnSkill()
		{
			fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(0);
			vm.DataSeries.ForecastedStaffing.Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleSkill30MinutesIntervalLengthAndTarget15()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(30, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));
			SkillDayRepository.Add(skillDay);

			var forecastedTask = skillDay.WorkloadDayCollection.First().TaskPeriodList
				.Single(t => t.Period.StartDateTime == latestStatsTime);
			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = forecastedTask.Period.StartDateTime,
					Calls = ((forecastedTask.TotalTasks / 2) * 1.2)
				}
				// 20% more calls than forecasted
			});

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.Time.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * 1.2);
		}

		[Test]
		public void ShouldSummariseForecastedStaffingForTwoSkills()
		{
			var scenario = fakeScenarioAndIntervalLength();

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);

			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));
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
			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45));
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
			{
				new SkillIntervalStatistics { SkillId = skill.Id.Value, StartTime = latestStatsTime, Calls = 30 }
			});

			var vm = Target.Load(new[] { skill.Id.Value });
			
			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.GreaterThan(0d);
		}

		[Test]
		public void ShouldNotShowUpdatedAndActualStaffingWhenNoStatistics()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45));
			SkillDayRepository.Add(skillDay);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.UpdatedForecastedStaffing.Should().Be.Empty();
			vm.DataSeries.ActualStaffing.Should().Be.Empty();
		}

        [Test]
        public void ShouldNotShowAnyStaffingDataWhenNoForecast()
        {
            var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
            var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

            Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
            var scenario = fakeScenarioAndIntervalLength();

            var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
            SkillRepository.Has(skill);

           IntradayQueueStatisticsLoader.Has(new List<SkillIntervalStatistics>
            {
                new SkillIntervalStatistics { SkillId = skill.Id.Value, StartTime = latestStatsTime, Calls = 30 }
            });

            var vm = Target.Load(new[] { skill.Id.Value });

            vm.DataSeries.ForecastedStaffing.Should().Be.Empty();
            vm.DataSeries.UpdatedForecastedStaffing.Should().Be.Empty();
            vm.DataSeries.ActualStaffing.Should().Be.Empty();
        }

        [Test]
		public void ShouldCalculateUpdatedForecastedStaffingCorrectlyForThreeSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 7, 45, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var scenario = fakeScenarioAndIntervalLength();

			var skillWithReforecast1 = createSkill(minutesPerInterval, "skill with reforecast 1", new TimePeriod(7,45,8,15));
			var skillWithReforecast2 = createSkill(minutesPerInterval, "skill with reforecast 2", new TimePeriod(7,45,8,15));
			var skillWithoutReforecast = createSkill(minutesPerInterval, "skill without reforecast", new TimePeriod(8, 0, 8, 15));


			var skillDayWithReforecast1 = createSkillDay(skillWithReforecast1, scenario, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 15));
			var skillDayWithReforecast2 = createSkillDay(skillWithReforecast2, scenario, Now.UtcDateTime(), new TimePeriod(7, 45, 8, 15));
			var skillDayWithoutReforecast = createSkillDay(skillWithoutReforecast, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));

			var deviationWithReforecast1 = 1.25d;
			var deviationWithReforecast2 = 1.5d;
			var noDeviationWithoutReforecast = 1.0d;
			var finalDeviationFactor = (deviationWithReforecast1 + deviationWithReforecast2 + noDeviationWithoutReforecast)/3;
			
			var actualCalls1 = getActualCallsPerSkillAndInterval(skillDayWithReforecast1.WorkloadDayCollection.First().TaskPeriodList.ToList(), skillWithReforecast1, deviationWithReforecast1, latestStatsTime);
			var actualCalls2 = getActualCallsPerSkillAndInterval(skillDayWithReforecast2.WorkloadDayCollection.First().TaskPeriodList.ToList(), skillWithReforecast2, deviationWithReforecast2, latestStatsTime);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skillWithReforecast1);
			SkillRepository.Has(skillWithReforecast2);
			SkillRepository.Has(skillWithoutReforecast);
			SkillDayRepository.Add(skillDayWithReforecast1);
			SkillDayRepository.Add(skillDayWithReforecast2);
			SkillDayRepository.Add(skillDayWithoutReforecast);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);
			
			var vm = Target.Load(new[] { skillWithReforecast1.Id.Value, skillWithReforecast2.Id.Value, skillWithoutReforecast.Id.Value });

			var expectedUpdatedStaffing = 
				skillDayWithoutReforecast.SkillStaffPeriodCollection.Last().FStaff + 
				skillDayWithReforecast1.SkillStaffPeriodCollection.Last().FStaff * deviationWithReforecast1 +
				skillDayWithReforecast2.SkillStaffPeriodCollection.Last().FStaff * deviationWithReforecast2;

			vm.DataSeries.UpdatedForecastedStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.UpdatedForecastedStaffing.First().Should().Be.EqualTo(null);
			vm.DataSeries.UpdatedForecastedStaffing.Last().Should().Be.EqualTo(expectedUpdatedStaffing);
			expectedUpdatedStaffing.Should().Be.EqualTo(vm.DataSeries.ForecastedStaffing.Last() * finalDeviationFactor);
		}

		[Test]
		public void ShouldReturnActualStaffingFromStartOfDayUntilLatestStatsTime()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Has(skill);

			var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30));
			SkillDayRepository.Add(skillDay);

			IntradayQueueStatisticsLoader.Has(getActualCallsPerSkillAndInterval(skillDay.WorkloadDayCollection.First().TaskPeriodList.ToList(), skill, 1.2d, latestStatsTime));

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

        [Test]
        public void ShouldReturnActualStaffingUpUntilLatestStatsTimeEvenWhenWeHaveStatsBeforeOpenHours()
        {
            var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
            var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
            Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

            var scenario = fakeScenarioAndIntervalLength();

            var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30));
            SkillRepository.Has(skill);

            var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30));
            SkillDayRepository.Add(skillDay);

            var skillStats = new List<SkillIntervalStatistics>
            {
                new SkillIntervalStatistics
                {
                    SkillId = skill.Id.Value,
                    StartTime = latestStatsTime.AddMinutes(-minutesPerInterval),
                    Calls = 123,
                    AverageHandleTime = 40d
                },
                new SkillIntervalStatistics
                {
                    SkillId = skill.Id.Value,
                    StartTime = latestStatsTime,
                    Calls = 123,
                    AverageHandleTime = 40d
                }
            };

            IntradayQueueStatisticsLoader.Has(skillStats);

            var vm = Target.Load(new[] { skill.Id.Value });

            vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(2);
            vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
            vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
        }

        [Test]
        public void ShouldReturnActualStaffingNotStartingFromStartOfOpen()
        {
            var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
            var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
            Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

            var scenario = fakeScenarioAndIntervalLength();

            var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 45));
            SkillRepository.Has(skill);

            var skillDay = createSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45));
            SkillDayRepository.Add(skillDay);

            var skillStats = new List<SkillIntervalStatistics>
            {
                new SkillIntervalStatistics
                {
                    SkillId = skill.Id.Value,
                    StartTime = latestStatsTime,
                    Calls = 123,
                    AverageHandleTime = 40d
                }
            };

            IntradayQueueStatisticsLoader.Has(skillStats);

            var vm = Target.Load(new[] { skill.Id.Value });

            vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(3);
            vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(null);
            vm.DataSeries.ActualStaffing[1].Should().Be.GreaterThan(0d);
            vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
        }

        [Test]
		public void ShouldSummariseActualStaffingForTwoSkills()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = fakeScenarioAndIntervalLength();

			var skill1 = createSkill(minutesPerInterval, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(minutesPerInterval, "skill2", new TimePeriod(8, 0, 8, 30));

			var skillDay1 = createSkillDay(skill1, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));
			var skillDay2 = createSkillDay(skill2, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 30));

			var actualCalls1 = getActualCallsPerSkillAndInterval(skillDay1.WorkloadDayCollection.First().TaskPeriodList.ToList(), skill1, 1.15, latestStatsTime);
			var actualCalls2 = getActualCallsPerSkillAndInterval(skillDay2.WorkloadDayCollection.First().TaskPeriodList.ToList(), skill2, 1.3, latestStatsTime);

			var actualCallsTotal = new List<SkillIntervalStatistics>();
			actualCallsTotal.AddRange(actualCalls1);
			actualCallsTotal.AddRange(actualCalls2);

			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			IntradayQueueStatisticsLoader.Has(actualCallsTotal);

			var calculatorService = new StaffingCalculatorServiceFacade();
			var skillData1 = skillDay1.SkillDataPeriodCollection.First().ServiceAgreement;
			var skillData2 = skillDay2.SkillDataPeriodCollection.First().ServiceAgreement;
			var actualStaffingSkill1 = calculatorService.AgentsUseOccupancy(
				skillData1.ServiceLevel.Percent.Value,
				(int)skillData1.ServiceLevel.Seconds,
				actualCalls1.First().Calls,
				actualCalls1.First().AverageHandleTime,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillData1.MinOccupancy.Value,
				skillData1.MaxOccupancy.Value,
				skill1.MaxParallelTasks);
			var actualStaffingSkill2 = calculatorService.AgentsUseOccupancy(
				skillData2.ServiceLevel.Percent.Value,
				(int)skillData2.ServiceLevel.Seconds,
				actualCalls2.First().Calls,
				actualCalls2.First().AverageHandleTime,
				TimeSpan.FromMinutes(minutesPerInterval),
				skillData2.MinOccupancy.Value,
				skillData2.MaxOccupancy.Value,
				skill1.MaxParallelTasks);

			var vm = Target.Load(new[] { skill1.Id.Value, skill2.Id.Value });

			vm.DataSeries.ActualStaffing.First().Should().Be.EqualTo(actualStaffingSkill1 + actualStaffingSkill2);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

	   

		private Scenario fakeScenarioAndIntervalLength()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);
			return scenario;
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

		private ISkillDay createSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours)
		{
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3, new Tuple<TimePeriod, double>(openHours, 3));
			var index = 0;

			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].Tasks = new Random().Next(5,50);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(200);
				index++;
			}

			return skillDay;
		}

		private List<SkillIntervalStatistics> getActualCallsPerSkillAndInterval(List<ITemplateTaskPeriod> taskPeriodList, ISkill skill, double deviationFactor, DateTime latestStatsTimeUtc)
		{
			var actualCallsList = new List<SkillIntervalStatistics>();
			foreach (var taskPeriod in taskPeriodList.Where(t => t.Period.StartDateTime <= latestStatsTimeUtc))
			{
				var actualCalls = taskPeriod.Task.Tasks * deviationFactor;
				actualCallsList.Add(new SkillIntervalStatistics
				{
					SkillId = skill.Id.Value,
					StartTime = TimeZoneHelper.ConvertFromUtc(taskPeriod.Period.StartDateTime, TimeZone.TimeZone()),
					Calls = actualCalls,
					AverageHandleTime = taskPeriod.Task.AverageHandlingTaskTime.TotalSeconds
				});
			}
			return actualCallsList;
		}

		private List<ITemplateTaskPeriod> forecastedTasksUpUntilLatestStatsTime(ISkill skill, Scenario scenario, DateTime userNow,
			DateTime latestStatsTimeUtc)
		{
			var userNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(userNow.Date, TimeZone.TimeZone());

			var skillYesterday = createSkillDay(skill, scenario, userNow.AddDays(-1), new TimePeriod(0, 0, 24, 0));
			var skillToday = createSkillDay(skill, scenario, userNow, new TimePeriod(0, 0, 24, 0));
			var skillTomorrow = createSkillDay(skill, scenario, userNow.AddDays(1), new TimePeriod(0, 0, 24, 0));
			SkillDayRepository.Add(skillYesterday);
			SkillDayRepository.Add(skillToday);
			SkillDayRepository.Add(skillTomorrow);

			var taskPeriodList = new List<ITemplateTaskPeriod>();
			taskPeriodList.AddRange(getTaskPeriods(skillYesterday, userNowStartOfDayUtc, latestStatsTimeUtc));
			taskPeriodList.AddRange(getTaskPeriods(skillToday, userNowStartOfDayUtc, latestStatsTimeUtc));
			taskPeriodList.AddRange(getTaskPeriods(skillTomorrow, userNowStartOfDayUtc, latestStatsTimeUtc));

			return taskPeriodList;
		}

		private IEnumerable<ITemplateTaskPeriod> getTaskPeriods(ISkillDay skillDay, DateTime userNowStartOfDayUtc, DateTime latestStatsTimeUtc)
		{
			return skillDay.WorkloadDayCollection.First().TaskPeriodList
				.Where(
					t =>
						t.Period.StartDateTime >= userNowStartOfDayUtc &&
						t.Period.EndDateTime <= latestStatsTimeUtc.AddMinutes(minutesPerInterval));
		}

	}
}