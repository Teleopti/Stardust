﻿using System;
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
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
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
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = StaffingViewModelCreatorTestHelper.CreateEmailSkill(_skillResolution, "skill", new TimePeriod(8, 0, 8, 45));
			
			var skillDay = StaffingViewModelCreatorTestHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 45), false, _slaTwoHours);
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

			SkillRepository.Has(skill);
			SkillDayRepository.Has(skillDay);
			IntradayQueueStatisticsLoader.Has(skillStats);

			var vm = Target.Load(new[] { skill.Id.Value });

			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(3);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(0d);
			vm.DataSeries.ActualStaffing[1].Should().Be.EqualTo(null);
			vm.DataSeries.ActualStaffing.Last().Should().Be.EqualTo(null);
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

			var scheduledStaffingList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval
				{
					SkillId = skillEmail.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(_skillResolution),
					StaffingLevel = 2
				},
				new SkillStaffingInterval
				{
					SkillId = skillEmail.Id.Value,
					StartDateTime = userNow.AddMinutes(_skillResolution),
					EndDateTime = userNow.AddMinutes(2*_skillResolution),
					StaffingLevel = 2
				},
			};

			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

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
		public void ShouldUseEmailsBacklogFromYesterdayForRequiredAgents()
		{
			var userNow = new DateTime(2016, 8, 26, 4, 0, 0, DateTimeKind.Utc);
			var latestStatsTime = new DateTime(2016, 8, 26, 4, 0, 0, DateTimeKind.Utc);
			var openHours = new TimePeriod(1, 0, 23, 0);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));

			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillEmail = StaffingViewModelCreatorTestHelper.CreateEmailSkill(_skillResolution, "skill", openHours);
			SkillRepository.Has(skillEmail);

			//var skillDayYesterday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow.AddDays(-1), openHours, false, _slaTwoHours);
			var skillDayToday = StaffingViewModelCreatorTestHelper.CreateSkillDay(skillEmail, scenario, userNow, openHours, false, _slaTwoHours, false);
			var skillDayCalculator = new SkillDayCalculator(skillEmail,
				new List<ISkillDay>() { skillDayToday},
				new DateOnlyPeriod(new DateOnly(userNow.AddDays(-1)), new DateOnly(userNow.AddDays(1))));
			//skillDayYesterday.SkillDayCalculator = skillDayCalculator;
			skillDayToday.SkillDayCalculator = skillDayCalculator;
			SkillDayRepository.Has(skillDayToday);

			var skillStats = StaffingViewModelCreatorTestHelper.CreateStatisticsBasedOnForecastedTasks(skillDayToday, latestStatsTime, minutesPerInterval, TimeZone.TimeZone());
			var backlogStats = createBacklogForIncomingEmails(new DateTime(2016, 8, 25, 23, 0, 0),
				new DateTime(2016, 8, 26, 1, 0, 0), skillEmail.Id.Value);
			IntradayQueueStatisticsLoader.Has(backlogStats);
			IntradayQueueStatisticsLoader.Has(skillStats);
			
			var forecastedAgentsStart = skillDayToday.SkillStaffPeriodCollection.First().FStaff;
			var forecastedAgentsSecondInterval = skillDayToday.SkillStaffPeriodCollection[1].FStaff;
			var forecastedAgentsThirdInterval = skillDayToday.SkillStaffPeriodCollection[2].FStaff;

			var vm = Target.Load(new[] { skillEmail.Id.Value });

			vm.DataSeries.Should().Not.Be.EqualTo(null);
			vm.StaffingHasData.Should().Be.EqualTo(true);
			vm.DataSeries.ActualStaffing.Length.Should().Be.EqualTo(88);
			vm.DataSeries.ActualStaffing.First().Should().Be.GreaterThan(forecastedAgentsStart);
			vm.DataSeries.ActualStaffing[4].Should().Be.GreaterThan(forecastedAgentsSecondInterval);
			vm.DataSeries.ActualStaffing[8].Should().Be.EqualTo(forecastedAgentsThirdInterval);
			vm.DataSeries.ActualStaffing[12].Should().Be.EqualTo(null);
		}

		private IList<SkillIntervalStatistics> createBacklogForIncomingEmails(DateTime startDateTime, DateTime endDateTime, Guid skillId)
		{
			var backLogStats = new List<SkillIntervalStatistics>();
			for (var interval = startDateTime; interval < endDateTime; interval = interval.AddMinutes(minutesPerInterval))
			{
				backLogStats.Add(new SkillIntervalStatistics(){
					Calls = 50,
					SkillId = skillId,
					StartTime = interval
				});
			}

			return backLogStats;
		}
	}
}
