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
	public class Bug81829
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
		public void Should_handle_agent_with_two_external_logons_for_schedule_changes()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var stockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZones = new UtcAndCetTimeZones();
			var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource) { BusinessUnitId = 1 };
			var intervals = new QuarterOfAnHourInterval();
			var scenario = new Scenario(1, TestState.BusinessUnit.Id.GetValueOrDefault(), true);
			var dayBeforeYesterday = new SpecificDate { Date = new DateOnly(2018, 5, 20), DateId = 0 };
			var yesterday = new SpecificDate { Date = new DateOnly(2018, 5, 21), DateId = 1 };
			var today = new SpecificDate { Date = new DateOnly(2018, 5, 22), DateId = 2 };
			var bridgeTimeZoneToday = new FillBridgeTimeZoneFromData(yesterday, intervals, timeZones, dataSource);
			var bridgeTimeZoneTomorrow = new FillBridgeTimeZoneFromData(today, intervals, timeZones, dataSource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;
			var personId = 1;
			var acdLoginId1 = 1;
			var acdLoginId2 = 2;
			var scheduleInterval = new ScheduledShift(personId, yesterday.DateId, yesterday.DateId, 31, 31, scenario.ScenarioId, dataSource, businessUnit.BusinessUnitId);
			var scheduleChange = new ScheduledChangesForDeviation(person.Id.Value, yesterday.Date.Date, scenario.ScenarioCode, 1, TestState.BusinessUnit.Id.Value);
			var agentStatsInterval = new FactAgent(yesterday.DateId, 31, acdLoginId1, 900, 900, 0, 0, 0, 0, 0, 0, 0);
			var statsUpUntilIntervalIdLocal = new IntervalBase(TimeZoneHelper.ConvertFromUtc(today.Date.Date.AddMinutes(32d * 15d), stockholmTimeZone), 96).Id;
			var logObjectDetail = new LogObjectDetail(today.Date.Date, statsUpUntilIntervalIdLocal);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(dataSource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Setup(today);
			analyticsDataFactory.Setup(dayBeforeYesterday);
			analyticsDataFactory.Setup(yesterday);
			analyticsDataFactory.Setup(bridgeTimeZoneToday);
			analyticsDataFactory.Setup(bridgeTimeZoneTomorrow);
			analyticsDataFactory.Setup(new Person(person, dataSource, personId, yesterday.Date.Date,
				new DateTime(2059, 12, 31), yesterday.DateId, -2, businessUnit.BusinessUnitId, TestState.BusinessUnit.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, Guid.NewGuid(), today.DateId));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId1, 1));
			analyticsDataFactory.Setup(new DefaultAcdLogin(acdLoginId2, 1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId1));
			analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(personId, acdLoginId2));
			analyticsDataFactory.Setup(scheduleInterval);
			analyticsDataFactory.Setup(scheduleChange);
			analyticsDataFactory.Setup(agentStatsInterval);
			analyticsDataFactory.Setup(logObjectDetail);
			analyticsDataFactory.Persist();


			var dateList = new JobMultipleDate(stockholmTimeZone);
			dateList.Add(yesterday.Date.Date, yesterday.Date.Date, JobCategoryType.AgentStatistics);
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
			JobStepBase step = new FactScheduleDeviationJobStepStory79646(jobParameters, true);
			var result = step.Run(new List<IJobStep>(), TestState.BusinessUnit, new List<IJobResult>(), true);
			result.Status.Should().Be("Done");

			Assert.That(SqlCommands.SumFactScheduleDeviation(yesterday.Date.Date, "deviation_contract_s"), Is.EqualTo(0));
		}

		private void clearSpecialAnalyticsData()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new LogObjectDetail(DateTime.MinValue, 0, true));
			analyticsDataFactory.Persist();
		}
	}
}