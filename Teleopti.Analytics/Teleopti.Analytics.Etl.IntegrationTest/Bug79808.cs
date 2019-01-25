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
			var existingScheduleDeviationToday = new FactScheduleDeviation(todayDateId, todayDateId, 95, personId, 900, 0, 0, 120, true, businessUnit.BusinessUnitId);
			var scheduledIntervalToday = new ScheduledShift(personId, todayDateId, 95, 95, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalTomorrow = new ScheduledShift(personId, tomorrowDateId, 0, 0, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
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

		private void clearSpecialAnalyticsData()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new LogObjectDetail(DateTime.MinValue, 0, true));
			analyticsDataFactory.Persist();
		}
	}
}