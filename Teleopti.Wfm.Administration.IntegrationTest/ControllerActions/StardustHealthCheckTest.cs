using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[TestFixture]
	public class StardustHealthCheckTest
	{
		public StardustController Target;
		public FakeStardustRepository StardustRepository;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new FakeStardustRepository();
			Target = new StardustController(StardustRepository, new AnotherTemporaryFakeStardustSender(StardustRepository), new FakeTenants(), new StaffingSettingsReader49Days(), new FakePingNode(),null);
		}

		[SetUp]
		public void SetUp()
		{
			StardustRepository.Clear();
		}
		
		[Test]
		public void ShouldBeHappyEverythingIsOk()
		{
			StardustRepository.Has(new WorkerNode());
			var response = Target.HealthCheck() as OkNegotiatedContentResult<string>;
			response.Content.Should().Contain("Everything looks OK!");
		}

		[Test]
		public void ShouldComplainIfNoNodesRegistered()
		{
			var response = Target.HealthCheck() as OkNegotiatedContentResult<string>;
			response.Content.Should().Contain("No nodes registered!");
		}

		[Test]
		public void ShouldComplainIfNoNodeAlive()
		{
			var node = new WorkerNode {Alive = false};
			StardustRepository.Has(node);
			var response = Target.HealthCheck() as OkNegotiatedContentResult<string>;
			response.Content.Should().Contain("No node is sending heartbeats.");
		}
	}
}