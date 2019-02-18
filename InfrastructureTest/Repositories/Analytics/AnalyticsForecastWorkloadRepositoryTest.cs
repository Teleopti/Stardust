using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsForecastWorkloadRepositoryTest
	{
		public IAnalyticsForecastWorkloadRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private BusinessUnit _businessUnit;

		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);
			_businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, _datasource, 1);

			var threeSkills = new ThreeSkills(_timeZones, _businessUnit, _datasource);

			analyticsDataFactory.Setup(new DatesFromPeriod(DateTime.Today, DateTime.Today+TimeSpan.FromDays(1)));
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(1, Guid.NewGuid()));
			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(_businessUnit);
			var workload = new AWorkload(threeSkills, _timeZones, _businessUnit, _datasource) {WorkloadId = 1};
			analyticsDataFactory.Setup(workload);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddForecastWorkload()
		{
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(new AnalyticsForcastWorkload
				{
					WorkloadId = 1,
					BusinessUnitId = 1,
					SkillId = 1,
					DatasourceUpdateDate = DateTime.Today,
					DateId = 0,
					EndTime = DateTime.Today,
					ForecastedAfterCallWorkExclCampaignSeconds = 1,
					ForecastedAfterCallWorkSeconds = 1,
					ForecastedBackofficeTasks = 1,
					ForecastedCalls = 1,
					ForecastedCallsExclCampaign = 1,
					ForecastedCampaignAfterCallWorkSeconds = 1,
					ForecastedCampaignCalls = 1,
					ForecastedCampaignHandlingTimeSeconds = 1,
					ForecastedCampaignTalkTimeSeconds = 1,
					ForecastedEmails = 1,
					ForecastedHandlingTimeExclCampaignSeconds = 1,
					ForecastedHandlingTimeSeconds = 1,
					ForecastedTalkTimeExclCampaignSeconds = 1,
					ForecastedTalkTimeSeconds = 1,
					IntervalId = 1,
					PeriodLengthMinutes = 15,
					ScenarioId = 1,
					StartTime = DateTime.Today
				});
			});
		}

	}
}