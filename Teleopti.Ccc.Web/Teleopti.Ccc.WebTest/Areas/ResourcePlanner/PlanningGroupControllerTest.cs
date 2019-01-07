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
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[DomainTest]
	public class PlanningGroupControllerTest : IExtendSystem
	{
		public PlanningGroupController Target;
		public FakePlanningGroupRepository PlanningGroupRepository;

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

			Target.Save(model);

			var planningGroup = PlanningGroupRepository.LoadAll().Single();
			planningGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldUpdatePlanningGroup()
		{
			var existingPlanningGroup = PlanningGroupRepository.Has(new PlanningGroup());
			var model = new PlanningGroupModel
			{
				Name = "UpdatedPlanningGroup",
				Id = existingPlanningGroup.Id.Value
			};

			Target.Save(model);

			var planningGroup = PlanningGroupRepository.LoadAll().Single();
			planningGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldFetchAll()
		{
			var planningGroup1 = new PlanningGroup();
			var planningGroup2 = new PlanningGroup();
			PlanningGroupRepository.Has(planningGroup1);
			PlanningGroupRepository.Has(planningGroup2);
			var result = Target.List().Result<IEnumerable<PlanningGroupModel>>().ToList();

			result.SingleOrDefault(x => x.Id == planningGroup1.Id.GetValueOrDefault()).Should().Not.Be.Null();
			result.SingleOrDefault(x => x.Id == planningGroup2.Id.GetValueOrDefault()).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFetchOne()
		{
			var planningGroup = new PlanningGroup {Name = RandomName.Make()};
			PlanningGroupRepository.Has(planningGroup);
			var result = Target.Get(planningGroup.Id.GetValueOrDefault()).Result<PlanningGroupModel>();

			result.Id.Should().Be.EqualTo(planningGroup.Id.GetValueOrDefault());
			result.Name.Should().Be.EqualTo(planningGroup.Name);
		}

		[Test]
		public void ShouldRemovePlanningGroup()
		{
			var planningGroup = PlanningGroupRepository.Has(new PlanningGroup());

			Target.DeletePlanningGroup(planningGroup.Id.Value);

			var result = PlanningGroupRepository.Get(planningGroup.Id.Value);
			result.IsDeleted.Should().Be.True();
		}
	}
}