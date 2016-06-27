using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Workload;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Workload
{
	[TestFixture]
	public class AnalyticsWorkloadUpdaterTest
	{
		private AnalyticsWorkloadUpdater _target;
		private Guid workloadId;
		private Guid businessUnitId;
		private IWorkloadRepository _workloadRepository;
		private IAnalyticsSkillRepository _analyticsSkillRepository;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private FakeAnalyticsWorkloadRepository _analyticsWorkloadRepository;

		[SetUp]
		public void Setup()
		{
			_workloadRepository = new FakeWorkloadRepository();
			_analyticsSkillRepository = new FakeAnalyticsSkillRepository();
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsWorkloadRepository = new FakeAnalyticsWorkloadRepository();

			_target = new AnalyticsWorkloadUpdater(_workloadRepository, _analyticsSkillRepository, _analyticsBusinessUnitRepository, _analyticsWorkloadRepository);
			workloadId = Guid.NewGuid();
			businessUnitId = Guid.NewGuid();
		}

		[Test]
		public void ShouldDoNothingWhenWorkloadMissing()
		{
			_target.Handle(new WorkloadChangedEvent {WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId});

			_analyticsWorkloadRepository.Workloads.Should().Be.Empty();
		}

		[Test, ExpectedException(typeof(BusinessUnitMissingInAnalyticsException))]
		public void ShouldThrowWhenBusinessUnitMissingFromAnalytics()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(workloadId);
			_workloadRepository.Add(workload);
			_analyticsBusinessUnitRepository.ReturnNull = true;

			_target.Handle(new WorkloadChangedEvent { WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId });
		}

		[Test, ExpectedException(typeof(SkillMissingInAnalyticsException))]
		public void ShouldThrowWhenSkillMissingFromAnalytics()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(workloadId);
			_workloadRepository.Add(workload);

			_target.Handle(new WorkloadChangedEvent { WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId });
		}

		[Test]
		public void ShouldAddWorkloadAndBridge()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			skill.SetId(Guid.NewGuid());
			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(workloadId);
			workload.QueueAdjustments = new QueueAdjustment();
			var queueMartId = 3;
			workload.AddQueueSource(new QueueSource("TestQueue", "Something", 1, 2, queueMartId, 4));
			_workloadRepository.Add(workload);
			var skillId = 123;
			var analyticsSkill = new AnalyticsSkill {SkillCode = skill.Id.GetValueOrDefault(), SkillId = skillId, SkillName = skill.Name, TimeZoneId = 12};
			_analyticsSkillRepository.AddOrUpdateSkill(analyticsSkill);
			_target.Handle(new WorkloadChangedEvent { WorkloadId = workloadId, LogOnBusinessUnitId = businessUnitId });

			_analyticsWorkloadRepository.Workloads.Should().Not.Be.Empty();
			var analyticsWorkload = _analyticsWorkloadRepository.Workloads.Single();
			analyticsWorkload.SkillCode.Should().Be.EqualTo(skill.Id.GetValueOrDefault());
			analyticsWorkload.SkillName.Should().Be.EqualTo(skill.Name);
			analyticsWorkload.SkillId.Should().Be.EqualTo(analyticsSkill.SkillId);
			analyticsWorkload.TimeZoneId.Should().Be.EqualTo(analyticsSkill.TimeZoneId);

			var bridgeQueueWorkloads = _analyticsWorkloadRepository.GetBridgeQueueWorkloads(analyticsWorkload.WorkloadId);
			bridgeQueueWorkloads.Should().Not.Be.Empty();
			var bridgeQueueWorkload = bridgeQueueWorkloads.Single();
			bridgeQueueWorkload.QueueId.Should().Be.EqualTo(queueMartId);
			bridgeQueueWorkload.SkillId.Should().Be.EqualTo(skillId);
			bridgeQueueWorkload.WorkloadId.Should().Be.EqualTo(analyticsWorkload.WorkloadId);
			
		}
	}
}