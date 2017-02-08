using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class AgentGroupControllerTest
	{
		[Test]
		public void ShouldCallPersister()
		{
			var model = new AgentGroupModel();
			var persister = MockRepository.GenerateMock<IAgentGroupModelPersister>();
			var target = new AgentGroupController(persister, null);

			target.Create(model);

			persister.AssertWasCalled(x => x.Persist(model));
		}

		[Test]
		public void ShouldFetchAll()
		{
			var model = new List<AgentGroupModel>();
			var fetchModel = MockRepository.GenerateMock<IFetchAgentGroupModel>();
			fetchModel.Expect(x => x.FetchAll()).Return(model);
			var target = new AgentGroupController(null, fetchModel);
			target.List().Result<IEnumerable<AgentGroupModel>>()
				.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldFetchOne()
		{
			var id = Guid.NewGuid();
			var model = new AgentGroupModel();

			var fetchModel = MockRepository.GenerateMock<IFetchAgentGroupModel>();
			fetchModel.Expect(x => x.Fetch(id)).Return(model);
			var target = new AgentGroupController(null, fetchModel);
			target.Get(id).Result<AgentGroupModel>()
				.Should().Be.SameInstanceAs(model);
		}
	}
}