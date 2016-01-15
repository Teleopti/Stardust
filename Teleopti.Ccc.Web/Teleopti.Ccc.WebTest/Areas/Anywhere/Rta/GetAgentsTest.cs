using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[IoCTest]
	[TestFixture]
	public class GetAgentsTest : ISetup
	{
		public IGetAgents Target;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<MutableNow>().For<INow>();
		}

		[Test]
		public void ShouldGetAgentForSites()
		{
			Now.Is("2015-10-21".Utc());
			var person = PersonFactory.CreatePerson("jobs").WithId();
			var team = TeamFactory.CreateTeamWithId("angel");
			var site = new Site("bla").WithId();
			site.AddTeam(team);
			SiteRepository.Has(site);
			PersonRepository.Has(person, team, "2015-10-21".Date());
		
			var agent = Target.ForSites(new[] {site.Id.Value});

			agent.Single().Name.Should().Be(person.Name.ToString());
			agent.Single().PersonId.Should().Be(person.Id);
			agent.Single().TeamId.Should().Be(team.Id.ToString());
			agent.Single().TeamName.Should().Be("angel");
			agent.Single().SiteId.Should().Be(site.Id.ToString());
			agent.Single().SiteName.Should().Be("bla");
		}

		[Test]
		public void ShouldGetAgentForTeams()
		{
			Now.Is("2015-10-28".Utc());
			var person = PersonFactory.CreatePerson("jobs").WithId();
			var team = TeamFactory.CreateTeamWithId("angel");
			var site = new Site("bla").WithId();
			team.Site = site;
			TeamRepository.Has(team);
			PersonRepository.Has(person, team, "2015-10-28".Date());

			var agent = Target.ForTeams(new[] { team.Id.Value });

			agent.Single().Name.Should().Be(person.Name.ToString());
			agent.Single().PersonId.Should().Be(person.Id);
			agent.Single().TeamId.Should().Be(team.Id.ToString());
			agent.Single().TeamName.Should().Be("angel");
			agent.Single().SiteId.Should().Be(site.Id.ToString());
			agent.Single().SiteName.Should().Be("bla");
		}
	}
}
