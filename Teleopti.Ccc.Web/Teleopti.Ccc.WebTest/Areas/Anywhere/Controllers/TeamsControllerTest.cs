using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class TeamsControllerTest
	{
		[Test]
		public void ShouldGetTeamsForSite()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new TeamsController(siteRepository, null);
			var site = new Site(" ");
			site.SetId(Guid.NewGuid());
			var team = new Team {Description = new Description("team1")};
			team.SetId(Guid.NewGuid());
			site.AddTeam(team);
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);

			var result = target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().Name.Should().Be("team1");
			result.Single().Id.Should().Be(team.Id.Value.ToString());
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			const int expected = 2;

			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInTeamReader>();
			var target = new TeamsController(siteRepository, numberOfAgentsQuery);
			var site = new Site(" ");
			site.SetId(Guid.NewGuid());
			var team = new Team { Description = new Description(" ") };
			team.SetId(Guid.NewGuid());
			site.AddTeam(team);
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { team })).Return(new Dictionary<Guid, int>() { { team.Id.Value, expected } });
			var result = target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().NumberOfAgents.Should().Be.EqualTo(expected);
		}
	}
}
