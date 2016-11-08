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
	public class AgentViewModelBuilderTest2
	{
		public AgentViewModelBuilder Target;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeCommonAgentNameProvider CommonAgentNameProvider;

		public FakeDatabase Database;
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

			var agent = Target.For(new ViewModelFilter {SiteIds = new[] { site.Id.Value } });

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

			var agent = Target.For(new ViewModelFilter {TeamIds = new[] { team.Id.Value } });

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

			var result = Target.For(new ViewModelFilter {SkillIds = new[] { skill } }).Single();

			result.PersonId.Should().Be(person);
			result.SiteId.Should().Be(site.Id.Value.ToString());
			result.SiteName.Should().Be("bla");
			result.TeamId.Should().Be(team.Id.Value.ToString());
			result.TeamName.Should().Be("angel");
			result.Name.Should().Be("123 - John Smith");
		}

		[Test]
		public void ShouldGetAgentForSkillAndTeam()
		{
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var red = TeamFactory.CreateTeamWithId("red");
			var green = TeamFactory.CreateTeamWithId("green");
			var paris = new Site("paris").WithId();
			paris.AddTeam(red);
			paris.AddTeam(green);
			SiteRepository.Has(paris);
			TeamRepository.Has(red);
			TeamRepository.Has(green);
			CommonAgentNameProvider
				.Has(new CommonNameDescriptionSetting { AliasFormat = "{EmployeeNumber} - {FirstName} {LastName}" });

			GroupingReadOnlyRepository
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = john,
					SiteId = paris.Id.Value,
					TeamId = red.Id.Value,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123"
				})
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = bill,
					SiteId = paris.Id.Value,
					TeamId = green.Id.Value,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124"
				});

			var result = Target.For(new ViewModelFilter { TeamIds = new[] { green.Id.Value }, SkillIds = new[] { skill }}).Single();

			result.PersonId.Should().Be(bill);
			result.SiteId.Should().Be(paris.Id.Value.ToString());
			result.SiteName.Should().Be("paris");
			result.TeamId.Should().Be(green.Id.Value.ToString());
			result.TeamName.Should().Be("green");
			result.Name.Should().Be("124 - Bill Gates");
		}

		[Test]
		public void ShouldGetAgentForSkillAndSite()
		{
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var london = new Site("london").WithId();
			var paris = new Site("paris").WithId();
			var red = TeamFactory.CreateTeamWithId("red");
			var students = TeamFactory.CreateTeamWithId("students");

			london.AddTeam(students);
			SiteRepository.Has(london);
			paris.AddTeam(red);
			SiteRepository.Has(paris);
			TeamRepository.Has(students);
			TeamRepository.Has(red);
			CommonAgentNameProvider
				.Has(new CommonNameDescriptionSetting { AliasFormat = "{EmployeeNumber} - {FirstName} {LastName}" });

			GroupingReadOnlyRepository
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = john,
					SiteId = paris.Id.Value,
					TeamId = red.Id.Value,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123"
				})
				.Has(new ReadOnlyGroupDetail
				{
					GroupId = skill,
					PersonId = bill,
					SiteId = london.Id.Value,
					TeamId = students.Id.Value,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124"
				});

			var result = Target.For(new ViewModelFilter {SiteIds = new[] { london.Id.Value }, SkillIds = new[] { skill }} ).Single();

			result.PersonId.Should().Be(bill);
			result.SiteId.Should().Be(london.Id.Value.ToString());
			result.SiteName.Should().Be("london");
			result.TeamId.Should().Be(students.Id.Value.ToString());
			result.TeamName.Should().Be("students");
			result.Name.Should().Be("124 - Bill Gates");
		}

		[Test]
		public void ShouldGetAgentForSkillArea()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Database
				.WithAgentNameDisplayedAs("{FirstName} {LastName}")
				.WithSite(siteId, "Paris")
				.WithTeam(teamId, "Angel")
				.WithAgent(personId1, "John Smith", teamId, siteId)
				.WithSkill(skill1)
				.InSkillGroupPage()
				.WithAgent(personId2, "Ashley Andeen", teamId, siteId)
				.WithSkill(skill2)
				.InSkillGroupPage()
				;

			var viewModel = Target.For(new ViewModelFilter {SkillIds = new[] { skill1, skill2 } });
			var person1 = viewModel.Single(p => p.PersonId == personId1);
			person1.SiteId.Should().Be(siteId.ToString());
			person1.SiteName.Should().Be("Paris");
			person1.TeamId.Should().Be(teamId.ToString());
			person1.TeamName.Should().Be("Angel");
			person1.Name.Should().Be("John Smith");
			var person2 = viewModel.Single(p => p.PersonId == personId2);
			person2.SiteId.Should().Be(siteId.ToString());
			person2.SiteName.Should().Be("Paris");
			person2.TeamId.Should().Be(teamId.ToString());
			person2.TeamName.Should().Be("Angel");
			person2.Name.Should().Be("Ashley Andeen");
		}
	}
}