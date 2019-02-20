using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
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
	public class IntradayQueueStatisticsLoaderTest
	{
		[Test]
		public void ShouldCheckThatStoredProcedureExistsForEmailBacklogForWorkload()
		{
			var target = new IntradayQueueStatisticsLoader();
			var emailBacklogWorkload = target.LoadActualEmailBacklogForWorkload(Guid.NewGuid(), new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date));

			emailBacklogWorkload.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadSkillVolumeStatisticsAndReturnCorrectStatistics()
		{
			var target = new IntradayQueueStatisticsLoader();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource, 1);
			var threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);
			var theSkill = new Skill("MySkill");
			theSkill.SetId(threeSkills.FirstSkillCode);
			var workload = new AWorkload(threeSkills, timeZones, businessUnit, datasource) { WorkloadId = 1 };
			var queue = new AQueue(datasource);
			var bridgeQueueWorkload = new FillBridgeQueueWorkload(threeSkills, businessUnit, datasource)
			{
				QueueId = queue.QueueId,
				WorkloadId = workload.WorkloadId
			};
			var intervals = new QuarterOfAnHourInterval();
			var date = new SpecificDate(){Date = new DateOnly(2018, 11, 26) };
			var utcBridgeTimeZoneDate = new FillBridgeTimeZoneFromData(date, intervals, timeZones, datasource);
			var scenario = new Scenario(1, Guid.NewGuid(), true);
			var queuStatsDate = new FactQueue(date, intervals, queue, datasource, utcBridgeTimeZoneDate, 95);

			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(datasource);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(workload);
			analyticsDataFactory.Setup(queue);
			analyticsDataFactory.Setup(bridgeQueueWorkload);
			analyticsDataFactory.Setup(intervals);
			analyticsDataFactory.Setup(date);
			analyticsDataFactory.Setup(utcBridgeTimeZoneDate);
			analyticsDataFactory.Setup(scenario);
			analyticsDataFactory.Setup(queuStatsDate);
			analyticsDataFactory.Persist();

			var result = target.LoadSkillVolumeStatistics(new List<ISkill> {theSkill}, new DateTime(2018, 11, 26));

			var actualAnsweredCalls = queuStatsDate.AnsweredCalls();
			var actualHandleTime = queuStatsDate.HandleTime();

			result.Sum(x => x.AnsweredCalls).Should().Be(actualAnsweredCalls);
			result.Sum(x => x.HandleTime).Should().Be(actualHandleTime);
		}
	}
}