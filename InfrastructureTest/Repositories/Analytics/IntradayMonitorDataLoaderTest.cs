using System;
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
			var date = new SpecificDate {Date = new DateOnly(1900, 1, 1), DateId = 1};
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

			var result = target.Load(new[] {threeSkills.FirstSkillCode}, TimeZoneInfo.Utc, new DateOnly(1900, 1, 1));

			result.Should().Not.Be.Empty();
			result.ForEach(x => x.ForecastedCalls.Should().Be.EqualTo(90));
			result.ForEach(x => x.ForecastedHandleTime.Should().Be.EqualTo(170));
		}
	}
}