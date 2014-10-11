using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class AgentBadgeProviderTest
	{
		[Test]
		public void ShouldQueryAllAgents()
		{
			var agentBadgerepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(new Guid[] {Guid.NewGuid()}))
				.IgnoreArguments()
				.Return(new IPerson[] {new Person()});

			var target = new AgentBadgeProvider(agentBadgerepository, MockRepository.GenerateMock<IPermissionProvider>(), personRepository);

			target.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			agentBadgerepository.AssertWasCalled(x => x.GetAgentToplistOfBadges());
		}

		[Test]
		public void ShouldFilterPermittedAgentsWhenQueryingAll()
		{
			var agentBadgeRepo = MockRepository.GenerateMock<IAgentBadgeRepository>();
			var personRepo = MockRepository.GenerateMock<IPersonRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var agents = new IAgentBadgeInfo[] {new AgentBadgeInfo(), new AgentBadgeInfo()};
			var persons = new IPerson[] { new Person(), new Person() };

			agentBadgeRepo.Stub(x => x.GetAgentToplistOfBadges()).Return(agents);
			personRepo.Stub(x => x.FindPeople(agents.Select(item => item.PersonId).ToList())).IgnoreArguments().Return(persons);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, persons.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, DateOnly.Today, persons.ElementAt(1))).Return(true);

			var target = new AgentBadgeProvider(agentBadgeRepo, permissionProvider, personRepo);

			var result = target.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			result.Single().Should().Be(persons.ElementAt(1));
		}
	}
}