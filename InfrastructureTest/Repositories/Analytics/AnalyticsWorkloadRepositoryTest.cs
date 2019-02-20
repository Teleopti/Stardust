using System;
using NUnit.Framework;
using SharpTestsEx;
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
	public class AnalyticsWorkloadRepositoryTest
	{
		public IAnalyticsWorkloadRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private BusinessUnit businessUnit;
		private ThreeSkills threeSkills;
		private AQueue queue;

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);

			threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);
			queue = new AQueue(datasource);

			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(queue);

			// Add "Not Defined" data
			analyticsDataFactory.Setup(new NotDefinedBusinessUnit());
			analyticsDataFactory.Setup(new NotDefinedSkill());
			analyticsDataFactory.Setup(new NotDefinedWorkload());

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldAddAndUpdateWorkload()
		{
			var random = new Random(DateTime.Now.Millisecond);
			var workloadCode = Guid.NewGuid();
			var workloadId = WithAnalyticsUnitOfWork.Get(() => Target.AddOrUpdate(new AnalyticsWorkload
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = DateTime.Now,
				ForecastMethodCode = Guid.NewGuid(),
				ForecastMethodName = "ForecastMethodName",
				IsDeleted = false,
				PercentageAbandoned = random.NextDouble() * 100,
				PercentageAbandonedAfterServiceLevel = random.NextDouble() * 100,
				PercentageAbandonedShort = random.NextDouble() * 100,
				PercentageAbandonedWithinServiceLevel = random.NextDouble() * 100,
				PercentageOffered = random.NextDouble() * 100,
				PercentageOverflowIn = random.NextDouble() * 100,
				PercentageOverflowOut = random.NextDouble() * 100,
				SkillCode = threeSkills.FirstSkillCode,
				SkillId = threeSkills.FirstSkillId,
				SkillName = threeSkills.FirstSkillName,
				TimeZoneId = 123,
				WorkloadCode = workloadCode,
				WorkloadName = "WorkloadName"
			}));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetWorkload(workloadCode));
			result.Should().Not.Be.Null();
			result.WorkloadId.Should().Be.EqualTo(workloadId);

			var workloadIdUpdated = WithAnalyticsUnitOfWork.Get(() => Target.AddOrUpdate(new AnalyticsWorkload
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = DateTime.Now,
				ForecastMethodCode = Guid.NewGuid(),
				ForecastMethodName = "ForecastMethodNameUpdated",
				IsDeleted = true,
				PercentageAbandoned = random.NextDouble() * 100,
				PercentageAbandonedAfterServiceLevel = random.NextDouble() * 100,
				PercentageAbandonedShort = random.NextDouble() * 100,
				PercentageAbandonedWithinServiceLevel = random.NextDouble() * 100,
				PercentageOffered = random.NextDouble() * 100,
				PercentageOverflowIn = random.NextDouble() * 100,
				PercentageOverflowOut = random.NextDouble() * 100,
				SkillCode = threeSkills.Skill2Code,
				SkillId = threeSkills.Skill2Id,
				SkillName = threeSkills.Skill2Name,
				TimeZoneId = 321,
				WorkloadCode = workloadCode,
				WorkloadName = "WorkloadNameUpdated"
			}));

			workloadIdUpdated.Should().Be.EqualTo(workloadId);

			result = WithAnalyticsUnitOfWork.Get(() => Target.GetWorkload(workloadCode));
			result.Should().Not.Be.Null();
			result.WorkloadId.Should().Be.EqualTo(workloadId);
		}

		[Test]
		public void CanAddAndRemoveBridgeQueueWorkloads()
		{
			var random = new Random(DateTime.Now.Millisecond);
			var workloadCode = Guid.NewGuid();
			var workloadId = WithAnalyticsUnitOfWork.Get(() => Target.AddOrUpdate(new AnalyticsWorkload
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = DateTime.Now,
				ForecastMethodCode = Guid.NewGuid(),
				ForecastMethodName = "ForecastMethodName",
				IsDeleted = false,
				PercentageAbandoned = random.NextDouble() * 100,
				PercentageAbandonedAfterServiceLevel = random.NextDouble() * 100,
				PercentageAbandonedShort = random.NextDouble() * 100,
				PercentageAbandonedWithinServiceLevel = random.NextDouble() * 100,
				PercentageOffered = random.NextDouble() * 100,
				PercentageOverflowIn = random.NextDouble() * 100,
				PercentageOverflowOut = random.NextDouble() * 100,
				SkillCode = threeSkills.FirstSkillCode,
				SkillId = threeSkills.FirstSkillId,
				SkillName = threeSkills.FirstSkillName,
				TimeZoneId = 123,
				WorkloadCode = workloadCode,
				WorkloadName = "WorkloadName"
			}));

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdateBridge(new AnalyticsBridgeQueueWorkload
				{
					BusinessUnitId = businessUnit.BusinessUnitId,
					QueueId = queue.QueueId,
					WorkloadId = workloadId,
					SkillId = threeSkills.FirstSkillId,
					DatasourceUpdateDate = DateTime.Now
				});
			});

			var bridges = WithAnalyticsUnitOfWork.Get(() => Target.GetBridgeQueueWorkloads(workloadId));
			bridges.Should().Not.Be.Empty();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.DeleteBridge(workloadId, queue.QueueId);
			});

			bridges = WithAnalyticsUnitOfWork.Get(() => Target.GetBridgeQueueWorkloads(workloadId));
			bridges.Should().Be.Empty();
		}
	}
}