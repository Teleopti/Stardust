using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class AgentGroupControllerTest
	{
		[Test]
		public void ShouldCallPersister()
		{
			var model = new AgentGroupModel();
			var persister = MockRepository.GenerateMock<IAgentGroupModelPersister>();
			var target = new AgentGroupController(null, persister);

			target.Create(model);

			persister.AssertWasCalled(x => x.Persist(model));
		}
	}
}