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
	public class AgentGroupControllerTest : ISetup
	{
		public AgentGroupController Target;
		public FakeAgentGroupRepository AgentGroupRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AgentGroupController>();
		}

		[Test]
		public void ShouldCreateAgentGroup()
		{
			var model = new AgentGroupModel
			{
				Name = "TestAgentGroup"
			};

			Target.Create(model);

			var agentGroup = AgentGroupRepository.LoadAll().Single();
			agentGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldUpdateAgentGroup()
		{
			var agentGroupId = Guid.NewGuid();
			AgentGroupRepository.Has(new AgentGroup("TestAgentGroup").WithId(agentGroupId));

			var model = new AgentGroupModel
			{
				Name = "UpdatedAgentGroup",
				Id = agentGroupId
			};

			Target.Create(model);

			var agentGroup = AgentGroupRepository.LoadAll().Single();
			agentGroup.Name.Should().Be.EqualTo(model.Name);
		}

		[Test]
		public void ShouldFetchAll()
		{
			var agentGroup1 = new AgentGroup("AgentGroup 1").WithId();
			var agentGroup2 = new AgentGroup("AgentGroup 2").WithId();
			AgentGroupRepository
				.Has(agentGroup1)
				.Has(agentGroup2);
			var result = Target.List().Result<IEnumerable<AgentGroupModel>>().ToList();

			result.SingleOrDefault(x => x.Id == agentGroup1.Id.GetValueOrDefault()).Should().Not.Be.Null();
			result.SingleOrDefault(x => x.Id == agentGroup2.Id.GetValueOrDefault()).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldFetchOne()
		{
			var agentGroup = new AgentGroup("AgentGroup 1").WithId();
			AgentGroupRepository
				.Has(agentGroup);
			var result = Target.Get(agentGroup.Id.GetValueOrDefault()).Result<AgentGroupModel>();

			result.Id.Should().Be.EqualTo(agentGroup.Id.GetValueOrDefault());
			result.Name.Should().Be.EqualTo(agentGroup.Name);
		}

		[Test]
		public void ShouldRemoveAgentGroup()
		{
			var agentGroupId = Guid.NewGuid();
			AgentGroupRepository
				.Has(new AgentGroup("AgentGroup 1").WithId(agentGroupId));

			Target.DeleteAgentGroup(agentGroupId);

			var result =(AgentGroup) AgentGroupRepository.Get(agentGroupId);
			result.IsDeleted.Should().Be.True();
		}
	}
}