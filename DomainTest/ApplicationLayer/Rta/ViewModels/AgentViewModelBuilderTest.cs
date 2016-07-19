using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentViewModelBuilderTest
	{
		public AgentViewModelBuilder Target;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeCommonAgentNameProvider CommonAgentNameProvider;
        public MutableNow Now;
		
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

		[Test]
		public void ShouldGetAgentForSkill()
		{
			var skill = Guid.NewGuid();
			var person = Guid.NewGuid();
			var team = TeamFactory.CreateTeamWithId("angel");
			var site = new Site("bla").WithId();
			site.AddTeam(team);
			SiteRepository.Has(site);
			TeamRepository.Has(team);
			CommonAgentNameProvider
				.Has(new CommonNameDescriptionSetting {AliasFormat = "{EmployeeNumber} - {FirstName} {LastName}"});

			GroupingReadOnlyRepository
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = person,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123"
				});

			var result = Target.ForSkill(skill).Single();

			result.PersonId.Should().Be(person);
			result.SiteId.Should().Be(site.Id.Value.ToString());
			result.SiteName.Should().Be("bla");
			result.TeamId.Should().Be(team.Id.Value.ToString());
			result.TeamName.Should().Be("angel");
			result.Name.Should().Be("123 - John Smith");
		}

		[Test]
		public void ShouldGetAgentForSkillArea()
		{
			var skill = Guid.NewGuid();
			var person = Guid.NewGuid();
			var team = TeamFactory.CreateTeamWithId("angel");
			var site = new Site("bla").WithId();
			site.AddTeam(team);
			SiteRepository.Has(site);
			TeamRepository.Has(team);
			CommonAgentNameProvider
				.Has(new CommonNameDescriptionSetting { AliasFormat = "{EmployeeNumber} - {FirstName} {LastName}" });

			GroupingReadOnlyRepository
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = person,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123"
				});

			var result = Target.ForSkill(skill).Single();

			result.PersonId.Should().Be(person);
			result.SiteId.Should().Be(site.Id.Value.ToString());
			result.SiteName.Should().Be("bla");
			result.TeamId.Should().Be(team.Id.Value.ToString());
			result.TeamName.Should().Be("angel");
			result.Name.Should().Be("123 - John Smith");
		}
	}
}
