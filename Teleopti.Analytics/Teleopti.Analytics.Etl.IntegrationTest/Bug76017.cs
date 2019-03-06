using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;


namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug76017
	{
		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		[TearDown]
		public void TearDown()
		{
			clearSpecialAnalyticsData();
			SetupFixtureForAssembly.EndTest();
			SqlCommands.EtlJobIntradaySettingsDelete();
		}

		[Test]
		public void Should_calculate_deviation_for_today_when_deviation_schedule_data_exists_for_tomorrow()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZones = new UtcAndCetTimeZones();
			var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource) {BusinessUnitId = 1};
			var intervals = new QuarterOfAnHourInterval();
			var scenario = new Scenario(1, TestState.BusinessUnit.Id.GetValueOrDefault(), true);
			var today = new SpecificDate { Date = new DateOnly(2018, 5, 21), DateId = 1 };
			var tomorrow = new SpecificDate { Date = new DateOnly(2018, 5, 22), DateId = 2 };
			var bridgeTimeZoneToday = new FillBridgeTimeZoneFromData(today, intervals, timeZones, dataSource);
			var bridgeTimeZoneTomorrow = new FillBridgeTimeZoneFromData(tomorrow, intervals, timeZones, dataSource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId = 1;
			var existingScheduleDeviationToday = new FactScheduleDeviation(today.DateId, today.DateId, today.DateId, 31, personId, 400, 0, 0, 500, true, businessUnit.BusinessUnitId);
			var existingScheduleDeviationTomorrow = new FactScheduleDeviation(tomorrow.DateId, tomorrow.DateId, tomorrow.DateId, 32, personId, 0, 0, 0, 0, false, businessUnit.BusinessUnitId);
			var scheduledIntervalToday1 = new ScheduledShift(personId, today.DateId, today.DateId, 31, 31, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalToday2 = new ScheduledShift(personId, today.DateId, today.DateId, 32, 32, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalTomorrow = new ScheduledShift(personId, tomorrow.DateId, tomorrow.DateId, 32, 32, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var agentStatsTodayInterval1 = new FactAgent(today.DateId, 31, acdLoginId, 400, 900, 500, 0, 0, 0, 0, 0, 0);
			var agentStatsTodayInterval2 = new FactAgent(today.DateId, 32, acdLoginId, 850, 900, 50, 0, 0, 0, 0, 0, 0);
			var statsUpUntilIntervalIdLocal = new IntervalBase(TimeZoneHelper.ConvertFromUtc(today.Date.Date.AddMinutes(32d * 15d), stockholmTimeZone), 96).Id;
			var logObjectDetail = new LogObjectDetail(today.Date.Date, statsUpUntilIntervalIdLocal);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(tomorrow);
			analyticsDataFactory.Setup(today);
			analyticsDataFactory.Setup(bridgeTimeZoneToday);
			analyticsDataFactory.Setup(bridgeTimeZoneTomorrow);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, today.Date.Date,
				new DateTime(2059, 12, 31), today.DateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), tomorrow.DateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));
			analyticsDataFactory.Setup(scheduledIntervalTomorrow);
			analyticsDataFactory.Setup(scheduledIntervalToday1);
			analyticsDataFactory.Setup(scheduledIntervalToday2);
			analyticsDataFactory.Setup(existingScheduleDeviationToday);
			analyticsDataFactory.Setup(existingScheduleDeviationTomorrow);
			analyticsDataFactory.Setup(agentStatsTodayInterval1);
			analyticsDataFactory.Setup(agentStatsTodayInterval2);
			analyticsDataFactory.Setup(logObjectDetail);
			analyticsDataFactory.Persist();

			
			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(today.Date.Date, today.Date.Date, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
				dateList, dataSource.RaptorDefaultDatasourceId, stockholmTimeZone.Id, 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString(), null, null), null),
				NowForTestPurpose = today.Date.Date.AddMinutes(33 * 15)
			};

			jobParameters.Helper.Repository.FillJobIntradaySettingsMart();
			Assert.That(SqlCommands.SumFactScheduleDeviation(today.Date.Date, "deviation_contract_s"), Is.EqualTo(500));

			JobStepBase step = new FactScheduleDeviationJobStep(jobParameters, true);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(today.Date.Date, "deviation_contract_s"), Is.EqualTo(550));
			var intradaySettings = SqlCommands.GetEtlJobIntradaySettingsValue(businessUnit.BusinessUnitId, -1, 4);
			Assert.That(intradaySettings.TargetDate, Is.EqualTo(today.Date));
			Assert.That(intradaySettings.TargetInterval, Is.EqualTo(32));

		}

		private void clearSpecialAnalyticsData()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new LogObjectDetail(DateTime.MinValue, 0, true));
			analyticsDataFactory.Persist();
		}

		[Test]
		public void Should_calculate_deviation_for_first_statistics_interval_of_day_when_last_deviation_is_from_the_day_before()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZones = new UtcAndCetTimeZones();
			var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource) {BusinessUnitId = 1};
			var intervals = new QuarterOfAnHourInterval();
			var scenario = new Scenario(1, TestState.BusinessUnit.Id.GetValueOrDefault(), true);
			var yesterday = new SpecificDate { Date = new DateOnly(2018, 5, 21), DateId = 30 };
			var today = new SpecificDate { Date = new DateOnly(2018, 5, 22), DateId = 31 };
			var plus1Day = new SpecificDate { Date = new DateOnly(2018, 5, 23), DateId = 32 };
			var bridgeTimeZoneYesterday = new FillBridgeTimeZoneFromData(yesterday, intervals, timeZones, dataSource);
			var bridgeTimeZoneToday = new FillBridgeTimeZoneFromData(today, intervals, timeZones, dataSource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			const int personId = 1;
			const int acdLoginId = 1;
			var existingScheduleDeviationYesterday = new FactScheduleDeviation(yesterday.DateId, yesterday.DateId, yesterday.DateId, 83, personId, 800, 0, 0, 100, true, businessUnit.BusinessUnitId);
			var scheduledIntervalToday = new ScheduledShift(personId, today.DateId, today.DateId, 32, 32, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var agentStatsTodayInterval = new FactAgent(today.DateId, 32, acdLoginId, 400, 900, 500, 0, 0, 0, 0, 0, 0);
			var statsUpUntilIntervalIdLocal = new IntervalBase(TimeZoneHelper.ConvertFromUtc(today.Date.Date.AddMinutes(32d * 15d), stockholmTimeZone), 96).Id;
			var logObjectDetail = new LogObjectDetail(today.Date.Date, statsUpUntilIntervalIdLocal);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(today);
			analyticsDataFactory.Setup(yesterday);
			analyticsDataFactory.Setup(plus1Day);
			analyticsDataFactory.Setup(bridgeTimeZoneToday);
			analyticsDataFactory.Setup(bridgeTimeZoneYesterday);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, yesterday.Date.Date,
				new DateTime(2059, 12, 31), yesterday.DateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), today.DateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));
			analyticsDataFactory.Setup(scheduledIntervalToday);
			analyticsDataFactory.Setup(existingScheduleDeviationYesterday);
			analyticsDataFactory.Setup(agentStatsTodayInterval);
			analyticsDataFactory.Setup(logObjectDetail);
			analyticsDataFactory.Persist();


			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(today.Date.Date, today.Date.Date, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
				dateList, dataSource.RaptorDefaultDatasourceId, stockholmTimeZone.Id, 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString(), null, null), null),
				NowForTestPurpose = today.Date.Date.AddMinutes(33 * 15)
			};

			jobParameters.Helper.Repository.FillJobIntradaySettingsMart();
			Assert.That(SqlCommands.SumFactScheduleDeviation(yesterday.Date.Date, "deviation_contract_s"), Is.EqualTo(100));

			JobStepBase step = new FactScheduleDeviationJobStep(jobParameters, true);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(today.Date.Date, "deviation_contract_s"), Is.EqualTo(500));
			var intradaySettings = SqlCommands.GetEtlJobIntradaySettingsValue(businessUnit.BusinessUnitId, -1, 4);
			Assert.That(intradaySettings.TargetDate, Is.EqualTo(today.Date));
			Assert.That(intradaySettings.TargetInterval, Is.EqualTo(32));
		}

		[Test]
		public void Should_calculate_deviation_not_leaving_gap_when_agent_stats_is_more_than_10_intervals_ahead()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZones = new UtcAndCetTimeZones();
			var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource) {BusinessUnitId = 1};
			var intervals = new QuarterOfAnHourInterval();
			var scenario = new Scenario(1, TestState.BusinessUnit.Id.GetValueOrDefault(), true);
			var today = new SpecificDate { Date = new DateOnly(2018, 5, 21), DateId = 30 };
			var plus1Day = new SpecificDate { Date = new DateOnly(2018, 5, 22), DateId = 31 };
			var datesExtraNeeded = new DatesFromPeriod(today.Date.AddDays(-11).Date, today.Date.Date);
			var bridgeTimeZoneToday = new FillBridgeTimeZoneFromData(today, intervals, timeZones, dataSource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId = 1;
			var scheduledIntervals = new ScheduledShift(personId, today.DateId, today.DateId, 32, 45, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var existingScheduleDeviationToday = new FactScheduleDeviation(today.DateId, today.DateId, today.DateId, 32, personId, 400, 0, 0, 500, true, businessUnit.BusinessUnitId);
			for (var intervalId = 32; intervalId <= 45; intervalId++)
			{
				var agentStatsInterval = new FactAgent(today.DateId, intervalId, acdLoginId, 800, 900, 100, 0, 0, 0, 0, 0, 0);
				analyticsDataFactory.Setup(agentStatsInterval);
			}
			var statsUpUntilIntervalIdLocal = new IntervalBase(TimeZoneHelper.ConvertFromUtc(today.Date.Date.AddMinutes(45d * 15d), stockholmTimeZone), 96).Id;
			var logObjectDetail = new LogObjectDetail(today.Date.Date, statsUpUntilIntervalIdLocal);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(today);
			analyticsDataFactory.Setup(plus1Day);
			analyticsDataFactory.Setup(datesExtraNeeded);
			analyticsDataFactory.Setup(bridgeTimeZoneToday);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, today.Date.Date,
				new DateTime(2059, 12, 31), today.DateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), today.DateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));
			analyticsDataFactory.Setup(scheduledIntervals);
			analyticsDataFactory.Setup(existingScheduleDeviationToday);
			analyticsDataFactory.Setup(logObjectDetail);
			analyticsDataFactory.Persist();


			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(today.Date.Date, today.Date.Date, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
				dateList, dataSource.RaptorDefaultDatasourceId, stockholmTimeZone.Id, 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString(), null, null), null),
				NowForTestPurpose = today.Date.Date.AddMinutes(46 * 15)
			};

			jobParameters.Helper.Repository.FillJobIntradaySettingsMart();
			Assert.That(SqlCommands.SumFactScheduleDeviation(today.Date.Date, "deviation_contract_s"), Is.EqualTo(500));

			JobStepBase step = new FactScheduleDeviationJobStep(jobParameters, true);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(today.Date.Date, "deviation_contract_s"), Is.EqualTo(14 * 100));
			var intradaySettings = SqlCommands.GetEtlJobIntradaySettingsValue(businessUnit.BusinessUnitId, -1, 4);
			Assert.That(intradaySettings.TargetDate, Is.EqualTo(today.Date));
			Assert.That(intradaySettings.TargetInterval, Is.EqualTo(45));
		}
	}
}