using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Workload;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Workload
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsWorkloadUpdaterTest : IExtendSystem
	{
		public AnalyticsWorkloadUpdater Target;
		public IWorkloadRepository WorkloadRepository;
		public IAnalyticsSkillRepository AnalyticsSkillRepository;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeAnalyticsWorkloadRepository AnalyticsWorkloadRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		private readonly Guid workloadId = Guid.NewGuid();
		private readonly Guid businessUnitId = Guid.NewGuid();
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsWorkloadUpdater>();
		}

		[Test]
		public void ShouldDoNothingWhenWorkloadMissing()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			Target.Handle(new WorkloadChangedEvent {WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId});

			AnalyticsWorkloadRepository.Workloads.Should().Be.Empty();
		}

		[Test]
		public void ShouldThrowWhenBusinessUnitMissingFromAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var skill = SkillFactory.CreateSkill("TestSkill");
			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(workloadId);
			WorkloadRepository.Add(workload);
			AnalyticsBusinessUnitRepository.ReturnNull = true;

			Assert.Throws<BusinessUnitMissingInAnalyticsException>(() =>
				Target.Handle(new WorkloadChangedEvent {WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId}));
		}

		[Test]
		public void ShouldThrowWhenSkillMissingFromAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var skill = SkillFactory.CreateSkill("TestSkill");
			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(workloadId);
			WorkloadRepository.Add(workload);

			Assert.Throws<SkillMissingInAnalyticsException>(() =>
				Target.Handle(new WorkloadChangedEvent {WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId}));
		}

		[Test]
		public void ShouldAddWorkloadAndBridge()
		{
			const int queueMartId = 3;
			const int skillId = 123;

			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var skill = SkillFactory.CreateSkill("TestSkill").WithId();
			var workload = WorkloadFactory.CreateWorkload(skill).WithId(workloadId);
			workload.QueueAdjustments = new QueueAdjustment();
			workload.AddQueueSource(new QueueSource("TestQueue", "Something", "1", "2", queueMartId, 4));
			WorkloadRepository.Add(workload);
			var analyticsSkill = new AnalyticsSkill
			{
				SkillCode = skill.Id.GetValueOrDefault(),
				SkillId = skillId,
				SkillName = skill.Name,
				TimeZoneId = 12
			};
			AnalyticsSkillRepository.AddOrUpdateSkill(analyticsSkill);
			Target.Handle(new WorkloadChangedEvent
			{
				WorkloadId = workloadId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsWorkloadRepository.Workloads.Should().Not.Be.Empty();
			var analyticsWorkload = AnalyticsWorkloadRepository.Workloads.Single();
			analyticsWorkload.SkillCode.Should().Be.EqualTo(skill.Id.GetValueOrDefault());
			analyticsWorkload.SkillName.Should().Be.EqualTo(skill.Name);
			analyticsWorkload.SkillId.Should().Be.EqualTo(analyticsSkill.SkillId);
			analyticsWorkload.TimeZoneId.Should().Be.EqualTo(analyticsSkill.TimeZoneId);

			var bridgeQueueWorkloads = AnalyticsWorkloadRepository.GetBridgeQueueWorkloads(analyticsWorkload.WorkloadId);
			bridgeQueueWorkloads.Should().Not.Be.Empty();
			var bridgeQueueWorkload = bridgeQueueWorkloads.Single();
			bridgeQueueWorkload.QueueId.Should().Be.EqualTo(queueMartId);
			bridgeQueueWorkload.SkillId.Should().Be.EqualTo(skillId);
			bridgeQueueWorkload.WorkloadId.Should().Be.EqualTo(analyticsWorkload.WorkloadId);
		}

		[Test]
		public void ShouldDeleteWorkloadAndConnectedBridge()
		{
			const int queueMartId = 3;
			const int skillId = 123;

			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var skill = SkillFactory.CreateSkill("TestSkill").WithId();
			var workload = WorkloadFactory.CreateWorkload(skill).WithId(workloadId);
			workload.QueueAdjustments = new QueueAdjustment();
			workload.AddQueueSource(new QueueSource("TestQueue", "Something", "1", "2", queueMartId, 4));
			((Domain.Forecasting.Workload) workload).SetDeleted();
			WorkloadRepository.Add(workload);

			var analyticsSkill = new AnalyticsSkill
			{
				SkillCode = skill.Id.GetValueOrDefault(),
				SkillId = skillId,
				SkillName = skill.Name,
				TimeZoneId = 12
			};
			AnalyticsSkillRepository.AddOrUpdateSkill(analyticsSkill);
			Target.Handle(new WorkloadChangedEvent
			{
				WorkloadId = workloadId,
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsWorkloadRepository.Workloads.Should().Not.Be.Empty();
			var analyticsWorkload = AnalyticsWorkloadRepository.Workloads.Single();
			analyticsWorkload.SkillCode.Should().Be.EqualTo(skill.Id.GetValueOrDefault());
			analyticsWorkload.SkillName.Should().Be.EqualTo(skill.Name);
			analyticsWorkload.SkillId.Should().Be.EqualTo(analyticsSkill.SkillId);
			analyticsWorkload.TimeZoneId.Should().Be.EqualTo(analyticsSkill.TimeZoneId);
			analyticsWorkload.IsDeleted.Should().Be.True();

			var bridgeQueueWorkloads = AnalyticsWorkloadRepository.GetBridgeQueueWorkloads(analyticsWorkload.WorkloadId);
			bridgeQueueWorkloads.Should().Be.Empty();
		}
	}
}