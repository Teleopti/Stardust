using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[DomainTest]
	public class PlanningGroupControllerTest : IExtendSystem
	{
		public PlanningGroupController Target;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PlanningGroupController>();
		}

		[Test]
		public void ShouldCreatePlanningGroup()
		{
			var model = new PlanningGroupModel
			{
				Name = "TestPlanningGroup"
			};

			Target.Create(model);

			var planningGroup = PlanningGroupRepository.LoadAll().Single();
			planningGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldUpdatePlanningGroup()
		{
			var planningGroupId = Guid.NewGuid();
			PlanningGroupRepository.Has(new PlanningGroup("TestPlanningGroup").WithId(planningGroupId));

			var model = new PlanningGroupModel
			{
				Name = "UpdatedPlanningGroup",
				Id = planningGroupId
			};

			Target.Create(model);

			var planningGroup = PlanningGroupRepository.LoadAll().Single();
			planningGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldFetchAll()
		{
			var planningGroup1 = new PlanningGroup("PlanningGroup 1").WithId();
			var planningGroup2 = new PlanningGroup("PlanningGroup 2").WithId();
			PlanningGroupRepository.Has(planningGroup1);
			PlanningGroupRepository.Has(planningGroup2);
			var result = Target.List().Result<IEnumerable<PlanningGroupModel>>().ToList();

			result.SingleOrDefault(x => x.Id == planningGroup1.Id.GetValueOrDefault()).Should().Not.Be.Null();
			result.SingleOrDefault(x => x.Id == planningGroup2.Id.GetValueOrDefault()).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFetchOne()
		{
			var planningGroup = new PlanningGroup("PlanningGroup 1").WithId();
			PlanningGroupRepository
				.Has(planningGroup);
			var result = Target.Get(planningGroup.Id.GetValueOrDefault()).Result<PlanningGroupModel>();

			result.Id.Should().Be.EqualTo(planningGroup.Id.GetValueOrDefault());
			result.Name.Should().Be.EqualTo(planningGroup.Name);
		}

		[Test]
		public void ShouldRemovePlanningGroup()
		{
			var planningGroupId = Guid.NewGuid();
			PlanningGroupRepository
				.Has(new PlanningGroup("PlanningGroup 1").WithId(planningGroupId));

			Target.DeletePlanningGroup(planningGroupId);

			var result = (PlanningGroup) PlanningGroupRepository.Get(planningGroupId);
			result.IsDeleted.Should().Be.True();
		}
	}
}