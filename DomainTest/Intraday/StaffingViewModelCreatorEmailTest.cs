using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class StaffingViewModelCreatorEmailTest : ISetup
	{
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public IStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;

		private int _skillResolution = 60;

		private readonly ServiceAgreement _slaTwoHours =
			new ServiceAgreement(new ServiceLevel(new Percent(1), 7200), new Percent(0), new Percent(1));

		private const int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		[Toggle(Toggles.Wfm_Intraday_SupportSkillTypeEmail_44002)]
		public void ShouldHandleActualStaffingForEmailSkillHavingStatsStartingBeforeOpenHour()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 30, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = StaffingViewModelCreatorTestHelper.CreateEmailSkill(15, "skill", new TimePeriod(8, 0, 9, 0));
			
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 9, 0), false, _slaTwoHours,false);
			var skillStats =
				StaffingViewModelCreatorTestHelper.CreateStatisticsBasedOnForecastedTasks(skillDay, latestStatsTime,
					minutesPerInterval, TimeZone.TimeZone());

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.HasStatistics(skillStats);

			var vm1 = Target.Load(new[] { skill.Id.Value });

			skillStats =
				StaffingViewModelCreatorTestHelper.CreateStatisticsBasedOnForecastedTasks(skillDay, latestStatsTime,
					minutesPerInterval, TimeZone.TimeZone());
			skillStats.Add(new SkillIntervalStatistics()
				{
					SkillId = skill.Id.Value,
					Calls = 20,
					AnsweredCalls = 16,
					HandleTime = 60,
					StartTime = latestStatsTime.AddMinutes(-75),
					WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value
				});
			skillStats.Add(new SkillIntervalStatistics()
			{
				SkillId = skill.Id.Value,
				Calls = 20,
				AnsweredCalls = 16,
				HandleTime = 60,
				StartTime = latestStatsTime.AddMinutes(-60),
				WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value
			});
			skillStats.Add(new SkillIntervalStatistics()
			{
				SkillId = skill.Id.Value,
				Calls = 20,
				AnsweredCalls = 16,
				HandleTime = 60,
				StartTime = latestStatsTime.AddMinutes(-45),
				WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value
			});
			skillStats.Add(new SkillIntervalStatistics()
			{
				SkillId = skill.Id.Value,
				Calls = 20,
				AnsweredCalls = 16,
				HandleTime = 60,
				StartTime = latestStatsTime.AddMinutes(-30),
				WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id.Value
			});

			IntradayQueueStatisticsLoader.HasStatistics(skillStats);
			var vm2 = Target.Load(new[] { skill.Id.Value });

			vm2.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(4);
			vm2.DataSeries.ActualStaffing.First().Should().Be.EqualTo(vm1.DataSeries.ActualStaffing.First());
			vm2.DataSeries.ActualStaffing[1].Should().Be.EqualTo(null);
			vm2.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
		}

		[Test]
		[Toggle(Toggles.Wfm_Intraday_SupportSkillTypeEmail_44002)]
		public void ShouldReturnForecastedStaffingForEmailSkill()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			
			var skillEmail = StaffingViewModelCreatorTestHelper.CreateEmailSkill(_skillResolution, "skill", new TimePeriod(8, 0, 10, 0));
			SkillRepository.Has(skillEmail);
			var skillDayYesterday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow.AddDays(-1), new TimePeriod(8, 0, 10, 0), false, _slaTwoHours);
			var skillDayToday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow, new TimePeriod(8, 0, 10, 0), false, _slaTwoHours);
			var skillDayTomorrow = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow.AddDays(1), new TimePeriod(8, 0, 10, 0), false, _slaTwoHours);
			var skillDayCalculator = new SkillDayCalculator(skillEmail,
				new List<ISkillDay>() { skillDayYesterday, skillDayToday, skillDayTomorrow },
				new DateOnlyPeriod(new DateOnly(userNow.AddDays(-1)), new DateOnly(userNow.AddDays(1))));
			skillDayYesterday.SkillDayCalculator = skillDayCalculator;
			skillDayToday.SkillDayCalculator = skillDayCalculator;
			skillDayTomorrow.SkillDayCalculator = skillDayCalculator;
			SkillDayRepository.Has(skillDayToday, skillDayTomorrow, skillDayYesterday);

			var skillCombinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					SkillCombination = new []{ skillEmail.Id.GetValueOrDefault() } ,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(_skillResolution),
					Resource = 2
				},
				new SkillCombinationResource
				{
					SkillCombination = new []{ skillEmail.Id.GetValueOrDefault() },
					StartDateTime = userNow.AddMinutes(_skillResolution),
					EndDateTime = userNow.AddMinutes(2*_skillResolution),
					Resource = 2
				},
			};

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), skillCombinationResources);

			var forecastedAgentsStart = skillDayToday.SkillStaffPeriodCollection.First().FStaff;
			var forecastedAgentsEnd = skillDayToday.SkillStaffPeriodCollection.Last().FStaff;

			var vm = Target.Load(new[] { skillEmail.Id.Value });

			vm.DataSeries.Should().Not.Be.EqualTo(null);
			vm.StaffingHasData.Should().Be.EqualTo(true);
			vm.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(8);
			vm.DataSeries.ForecastedStaffing.First().Should().Not.Be.EqualTo(forecastedAgentsStart);
			vm.DataSeries.ForecastedStaffing.Last().Should().Not.Be.EqualTo(forecastedAgentsEnd);
		}


		[Test]
		[Toggle(Toggles.Wfm_Intraday_SupportSkillTypeEmail_44002)]
		public void ShouldUseStatisticsBacklogForRequiredAgents()
		{
			var userNow = new DateTime(2016, 8, 26, 4, 15, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 4, 0, 0, DateTimeKind.Utc);
			var openHours = new TimePeriod(1, 0, 5, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillEmail = StaffingViewModelCreatorTestHelper.CreateEmailSkill(_skillResolution, "skill", openHours);
			SkillRepository.Has(skillEmail);

			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow, openHours, false, _slaTwoHours, false);
			var skillDayCalculator = new SkillDayCalculator(skillEmail,
				new List<ISkillDay>() { skillDay },
				new DateOnlyPeriod(new DateOnly(userNow), new DateOnly(userNow)));
			skillDay.SkillDayCalculator = skillDayCalculator;
			SkillDayRepository.Has(skillDay);

			var skillStats = StaffingViewModelCreatorTestHelper.CreateStatisticsBasedOnForecastedTasks(skillDay, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			IntradayQueueStatisticsLoader.HasStatistics(skillStats);
			
			var vm1 = Target.Load(new[] { skillEmail.Id.Value });

			IntradayQueueStatisticsLoader.HasBacklog(skillDay.WorkloadDayCollection.First().Workload.Id.Value, 300);

			var vm2 = Target.Load(new[] { skillEmail.Id.Value });

			vm2.DataSeries.Should().Not.Be.EqualTo(null);
			vm2.StaffingHasData.Should().Be.EqualTo(true);
			vm2.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(16);
			vm2.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(vm1.DataSeries.ActualStaffing.First());
			vm2.DataSeries.ActualStaffing[4].Should().Be.GreaterThan(vm1.DataSeries.ActualStaffing[4]);
			vm2.DataSeries.ActualStaffing[8].Should().Be.EqualTo(vm1.DataSeries.ActualStaffing[8]);
			vm2.DataSeries.ActualStaffing[12].Should().Be.EqualTo(null);
		}
	}
}
