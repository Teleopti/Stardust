using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class IntradayMonitorDataLoaderTest
	{
		[Test]
		public void ShouldCallLoadWithNoRowsReturned()
		{
			var intradayMonitorDataLoader = new IntradayMonitorDataLoader();
			var data = intradayMonitorDataLoader.Load(new[] {Guid.NewGuid()}, TimeZoneInfo.Utc, DateOnly.Today);

			data.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotLoadDataForDeletedWorkloads()
		{
			var target = new IntradayMonitorDataLoader();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource, 1);
			var threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);
			var workload = new AWorkload(threeSkills, timeZones, businessUnit, datasource) { WorkloadId = 1 };
			var deletedWorkload = new AWorkload(threeSkills, timeZones, businessUnit, datasource) { WorkloadId = 2, IsDeleted = true};
			var intervals = new QuarterOfAnHourInterval();
			var date = new SpecificDate {Date = new DateOnly(1900, 1, 2), DateId = 1};
			var bridgeTimeZone = new FillBridgeTimeZoneFromData(date, intervals, timeZones, datasource);
			var scenario = new Scenario(1, Guid.NewGuid(), true);
			var factForecastWorkloads1 = new FillFactForecastWorkload(date, intervals, threeSkills)
			{
				WorkloadId = 1,
				ScenarioId = 1,
				ForecastedCalls = 90,
				ForecastedHandlingTimeSeconds = 170
			};

			var factForecastWorkloads2 = new FillFactForecastWorkload(date, intervals, threeSkills)
			{
				WorkloadId = 2,
				ScenarioId = 1,
				ForecastedCalls = 50,
				ForecastedHandlingTimeSeconds = 100
			};
			
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(datasource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(workload);
			analyticsDataFactory.Setup(deletedWorkload);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(date);
			analyticsDataFactory.Setup(bridgeTimeZone);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(factForecastWorkloads1);
			analyticsDataFactory.Setup(factForecastWorkloads2);
			analyticsDataFactory.Persist();

			var result = target.Load(new[] {threeSkills.FirstSkillCode}, TimeZoneInfo.Utc, new DateOnly(1900, 1, 2));

			result.Should().Not.Be.Empty();
			result.ForEach(x => x.ForecastedCalls.Should().Be.EqualTo(90));
			result.ForEach(x => x.ForecastedHandleTime.Should().Be.EqualTo(170));
		}

		[Test]
		public void ShouldHandleDateIdGaps()
		{
			var target = new IntradayMonitorDataLoader();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var easternStandardTimeZone = new ATimeZone("Eastern Standard Time") {TimeZoneId = 2};
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource, 1);
			var threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);
			var workload = new AWorkload(threeSkills, timeZones, businessUnit, datasource) { WorkloadId = 1 };
			var queue = new AQueue(datasource);
			var bridgeQueueWorkload = new FillBridgeQueueWorkload(threeSkills, businessUnit, datasource)
			{
				QueueId = queue.QueueId,
				WorkloadId = workload.WorkloadId
			};
			var intervals = new QuarterOfAnHourInterval();
			var dates = new DatesFromPeriod(new DateTime(2018, 11, 25), new DateTime(2018, 11, 29)) {CreateDateIdGap = true};
			var utcBridgeTimeZoneDate = new FillBridgeTimeZoneFromData(dates, intervals, timeZones, datasource);
			var easternBridgeTimeZoneDate = new FillBridgeTimeZoneFromData(dates, intervals, easternStandardTimeZone, datasource);
			var scenario = new Scenario(1, Guid.NewGuid(), true);
			var forecastData = new FillFactForecastWorkload(dates, intervals, threeSkills)
			{
				WorkloadId = 1,
				ScenarioId = 1,
				ForecastedCalls = 90,
				ForecastedHandlingTimeSeconds = 170
			};
			var queuStatsDate = new FactQueue(dates, intervals, queue, datasource, utcBridgeTimeZoneDate, 95);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(easternStandardTimeZone);
			analyticsDataFactory.Setup(datasource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(workload);
			analyticsDataFactory.Setup(queue);
			analyticsDataFactory.Setup(bridgeQueueWorkload);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(utcBridgeTimeZoneDate);
			analyticsDataFactory.Setup(easternBridgeTimeZoneDate);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(forecastData);
			analyticsDataFactory.Setup(queuStatsDate);
			analyticsDataFactory.Persist();

			var result = target.Load(new[] { threeSkills.FirstSkillCode }, easternStandardTimeZone.TimeZoneInfo, new DateOnly(2018, 11, 26));

			result.Count.Should().Be(96);
			result.All(x => x.AnsweredCalls.HasValue).Should().Be.True();
			result.All(x => x.HandleTime.HasValue).Should().Be.True();
		}
	}
}