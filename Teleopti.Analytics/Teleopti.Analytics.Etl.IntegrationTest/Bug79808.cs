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
		public void Should_handle_date_id_gaps_and_not_remove_existing_deviation_for_next_UTC_day_running_non_intraday_step()
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
			var dates = new DatesFromPeriod(day1.AddDays(-1), day2.AddDays(2)) {CreateDateIdGap = true};
			var day1DateId = 2;
			var day2DateId = 4;
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId = 1;
			var existingScheduleDeviationDay1 = new FactScheduleDeviation(day1DateId, day1DateId, day1DateId, 95, personId, 900, 0, 0, 120, true, businessUnit.BusinessUnitId);
			var existingScheduleDeviationDay2 = new FactScheduleDeviation(day1DateId, day1DateId, day2DateId, 0, personId, 900, 0, 0, 500, true, businessUnit.BusinessUnitId);
			var scheduledIntervalDay1 = new ScheduledShift(personId, day1DateId, day1DateId, 95, 95, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduledIntervalDay2 = new ScheduledShift(personId, day1DateId, day2DateId, 0, 0, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
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