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
	public class Bug79808
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
			var today = new DateTime(2018, 11, 26);
			var tomorrow = new DateTime(2018, 11, 27);
			var dates = new DatesFromPeriod(today, tomorrow.AddDays(1));
			var todayDateId = 0;
			var tomorrowDateId = 1;
			var bridgeTimeZone = new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId = 1;
			var existingScheduleDeviationToday = new FactScheduleDeviation(todayDateId, todayDateId, todayDateId, 95, personId, 900, 0, 0, 120, true, businessUnit.BusinessUnitId);
			var scheduledIntervalToday = new ScheduledShift(personId, todayDateId, todayDateId, 95, 95, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalTomorrow = new ScheduledShift(personId, tomorrowDateId, tomorrowDateId, 0, 0, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var agentStatsTodayInterval1 = new FactAgent(todayDateId, 95, acdLoginId, 780, 900, 120, 0, 0, 0, 0, 0, 0);
			var agentStatsTodayInterval2 = new FactAgent(tomorrowDateId, 0, acdLoginId, 400, 900, 500, 0, 0, 0, 0, 0, 0);
			var statsUpUntilIntervalIdLocal = new IntervalBase(TimeZoneHelper.ConvertFromUtc(tomorrow.AddMinutes(0d * 15d), stockholmTimeZone), 96).Id;
			var logObjectDetail = new LogObjectDetail(tomorrow, statsUpUntilIntervalIdLocal);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(bridgeTimeZone);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, today,
				new DateTime(2059, 12, 31), todayDateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), tomorrowDateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));
			analyticsDataFactory.Setup(scheduledIntervalTomorrow);
			analyticsDataFactory.Setup(scheduledIntervalToday);
			analyticsDataFactory.Setup(existingScheduleDeviationToday);
			analyticsDataFactory.Setup(agentStatsTodayInterval1);
			analyticsDataFactory.Setup(agentStatsTodayInterval2);
			analyticsDataFactory.Setup(logObjectDetail);
			analyticsDataFactory.Persist();

			
			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(today, today, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
				dateList, dataSource.RaptorDefaultDatasourceId, stockholmTimeZone.Id, 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null),
				NowForTestPurpose = tomorrow.AddMinutes(1 * 15)
			};

			jobParameters.Helper.Repository.FillJobIntradaySettingsMart();

			Assert.That(SqlCommands.SumFactScheduleDeviation(today, "deviation_contract_s"), Is.EqualTo(120));

			JobStepBase step = new FactScheduleDeviationJobStep(jobParameters, true);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(tomorrow, "deviation_contract_s"), Is.EqualTo(500));
			var intradaySettings = SqlCommands.GetEtlJobIntradaySettingsValue(businessUnit.BusinessUnitId, -1, 4);
			Assert.That(intradaySettings.TargetDate, Is.EqualTo(new DateOnly(tomorrow)));
			Assert.That(intradaySettings.TargetInterval, Is.EqualTo(0));

		}

		[Test]
		public void Should_not_remove_existing_deviation_for_next_UTC_day()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZones = new UtcAndCetTimeZones();
			var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource) { BusinessUnitId = 1 };
			var intervals = new QuarterOfAnHourInterval();
			var scenario = new Scenario(1, TestState.BusinessUnit.Id.GetValueOrDefault(), true);
			var day1 = new DateTime(2018, 11, 26);
			var day2 = new DateTime(2018, 11, 27);
			var dates = new DatesFromPeriod(day1, day2.AddDays(1));
			var day1DateId = 0;
			var day2DateId = 1;
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId = 1;
			var localShiftStartIdDateUtcMinus5 = 0;
			var existingScheduleDeviationDay1 = new FactScheduleDeviation(localShiftStartIdDateUtcMinus5, day1DateId, day1DateId, 95, personId, 900, 0, 0, 120, true, businessUnit.BusinessUnitId);
			var existingScheduleDeviationDay2 = new FactScheduleDeviation(localShiftStartIdDateUtcMinus5, day1DateId, day2DateId, 0, personId, 900, 0, 0, 500, true, businessUnit.BusinessUnitId);
			var scheduledIntervalDay1 = new ScheduledShift(personId, localShiftStartIdDateUtcMinus5, day1DateId, 95, 95, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalDay2 = new ScheduledShift(personId, localShiftStartIdDateUtcMinus5, day2DateId, 0, 0, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var agentStatsDay1Interval1 = new FactAgent(day1DateId, 95, acdLoginId, 700, 900, 200, 0, 0, 0, 0, 0, 0);
			var agentStatsDay2Interval1 = new FactAgent(day2DateId, 0, acdLoginId, 400, 900, 500, 0, 0, 0, 0, 0, 0);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, day1,
				new DateTime(2059, 12, 31), day1DateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), day2DateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId));
			analyticsDataFactory.Setup(scheduledIntervalDay2);
			analyticsDataFactory.Setup(scheduledIntervalDay1);
			analyticsDataFactory.Setup(existingScheduleDeviationDay1);
			analyticsDataFactory.Setup(existingScheduleDeviationDay2);
			analyticsDataFactory.Setup(agentStatsDay1Interval1);
			analyticsDataFactory.Setup(agentStatsDay2Interval1);
			analyticsDataFactory.Persist();


			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(day1, day1, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
				dateList, dataSource.RaptorDefaultDatasourceId, stockholmTimeZone.Id, 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null)
			};

			Assert.That(SqlCommands.SumFactScheduleDeviation(day1, "deviation_contract_s"), Is.EqualTo(620));

			JobStepBase step = new FactScheduleDeviationJobStep(jobParameters, false);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(day1, "deviation_contract_s"), Is.EqualTo(700));
		}

		private void clearSpecialAnalyticsData()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new LogObjectDetail(DateTime.MinValue, 0, true));
			analyticsDataFactory.Persist();
		}
	}
}