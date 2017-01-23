using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class AgentStateNameTest
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Persister;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetAgentForSite()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");
			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "123",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Team Perferences",
				});

			var agent = Target.For(new ViewModelFilter { SiteIds = new[] { siteId } }).States.Single();

			agent.Name.Should().Be("123 - Bill Gates");
			agent.PersonId.Should().Be(personId);
			agent.TeamId.Should().Be(teamId.ToString());
			agent.TeamName.Should().Be("Team Perferences");
			agent.SiteId.Should().Be(siteId.ToString());
			agent.SiteName.Should().Be("London");
		}

		[Test]
		public void ShouldGetAgentForTeam()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");
			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "123",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Team Perferences",
				});

			var agent = Target.For(new ViewModelFilter { TeamIds = new[] { teamId } }).States.Single();

			agent.Name.Should().Be("123 - Bill Gates");
			agent.PersonId.Should().Be(personId);
			agent.TeamId.Should().Be(teamId.ToString());
			agent.TeamName.Should().Be("Team Perferences");
			agent.SiteId.Should().Be(siteId.ToString());
			agent.SiteName.Should().Be("London");
		}

		[Test]
		public void ShouldGetAgentForSkill()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");
			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "123",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Team Perferences",

				})
				.WithPersonSkill(personId, skillId);

			var agent = Target.For(new ViewModelFilter { SkillIds = new[] { skillId } }).States.Single();

			agent.PersonId.Should().Be(personId);
			agent.SiteId.Should().Be(siteId.ToString());
			agent.SiteName.Should().Be("London");
			agent.TeamId.Should().Be(teamId.ToString());
			agent.TeamName.Should().Be("Team Perferences");
			agent.Name.Should().Be("123 - Bill Gates");
		}

		[Test]
		public void ShouldGetAgentForSiteAndTeam()
		{
			var paris = Guid.NewGuid();
			var red = Guid.NewGuid();
			var london = Guid.NewGuid();
			var students = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");

			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = john,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123",
					SiteId = paris,
					SiteName = "Paris",
					TeamId = red,
					TeamName = "Red",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = bill,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124",
					SiteId = london,
					SiteName = "London",
					TeamId = students,
					TeamName = "Students",

				});

			var viewModel = Target.For(new ViewModelFilter { SiteIds = new[] { london }, TeamIds = new[] { red } });

			var johnVm = viewModel.States.Single(p => p.PersonId == john);
			johnVm.SiteId.Should().Be(paris.ToString());
			johnVm.SiteName.Should().Be("Paris");
			johnVm.TeamId.Should().Be(red.ToString());
			johnVm.TeamName.Should().Be("Red");
			johnVm.Name.Should().Be("123 - John Smith");

			var billVm = viewModel.States.Single(p => p.PersonId == bill);
			billVm.SiteId.Should().Be(london.ToString());
			billVm.SiteName.Should().Be("London");
			billVm.TeamId.Should().Be(students.ToString());
			billVm.TeamName.Should().Be("Students");
			billVm.Name.Should().Be("124 - Bill Gates");

		}

		[Test]
		public void ShouldGetAgentForSkillAndTeam()
		{
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var paris = Guid.NewGuid();
			var red = Guid.NewGuid();
			var london = Guid.NewGuid();
			var students = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");

			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = john,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123",
					SiteId = paris,
					SiteName = "Paris",
					TeamId = red,
					TeamName = "Red",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = bill,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124",
					SiteId = london,
					SiteName = "London",
					TeamId = students,
					TeamName = "Students",

				}).WithPersonSkill(bill, skill);
			var agent = Target.For(new ViewModelFilter { TeamIds = new[] { students }, SkillIds = new[] { skill } }).States.Single();

			agent.PersonId.Should().Be(bill);
			agent.SiteId.Should().Be(london.ToString());
			agent.SiteName.Should().Be("London");
			agent.TeamId.Should().Be(students.ToString());
			agent.TeamName.Should().Be("Students");
			agent.Name.Should().Be("124 - Bill Gates");
		}


		[Test]
		public void ShouldGetAgentForSkillAndSite()
		{
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var paris = Guid.NewGuid();
			var red = Guid.NewGuid();
			var london = Guid.NewGuid();
			var students = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");

			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = john,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123",
					SiteId = paris,
					SiteName = "Paris",
					TeamId = red,
					TeamName = "Red",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = bill,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124",
					SiteId = london,
					SiteName = "London",
					TeamId = students,
					TeamName = "Students",

				}).WithPersonSkill(bill, skill);
			var agent = Target.For(new ViewModelFilter { SiteIds = new[] { london }, SkillIds = new[] { skill } }).States.Single();

			agent.PersonId.Should().Be(bill);
			agent.SiteId.Should().Be(london.ToString());
			agent.SiteName.Should().Be("London");
			agent.TeamId.Should().Be(students.ToString());
			agent.TeamName.Should().Be("Students");
			agent.Name.Should().Be("124 - Bill Gates");
		}

		[Test]
		public void ShouldGetAgentForSkillSiteAndTeam()
		{
			var paris = Guid.NewGuid();
			var parisTeam = Guid.NewGuid();
			var london = Guid.NewGuid();
			var londonTeam1 = Guid.NewGuid();
			var londonTeam2 = Guid.NewGuid();
			var sales = Guid.NewGuid();
			var support = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var pierre = Guid.NewGuid();
			var ashley = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");

			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = john,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123",
					SiteId = paris,
					SiteName = "Paris",
					TeamId = parisTeam,
					TeamName = "ParisTeam",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = bill,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124",
					SiteId = london,
					SiteName = "London",
					TeamId = londonTeam1,
					TeamName = "LondonTeam1",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = pierre,
					FirstName = "Pierre",
					LastName = "Baldi",
					EmploymentNumber = "125",
					SiteId = london,
					SiteName = "London",
					TeamId = londonTeam1,
					TeamName = "LondonTeam1",
				})
				.Has(new AgentStateReadModel
				{
					PersonId = ashley,
					FirstName = "Ashley",
					LastName = "Andeen",
					EmploymentNumber = "126",
					SiteId = london,
					SiteName = "LondonTeam2",
					TeamId = londonTeam2,
					TeamName = "LondonTeam2",

				})
				.WithPersonSkill(john, support)
				.WithPersonSkill(bill, support)
				.WithPersonSkill(pierre, sales)
				.WithPersonSkill(ashley, support);

			var agent = Target.For(new ViewModelFilter { SiteIds = new[] { paris }, TeamIds = new[] { londonTeam1 }, SkillIds = new[] { support } }).States;

			var johnSmithModel = agent.Single(x => x.PersonId == john);
			johnSmithModel.SiteId.Should().Be(paris.ToString());
			johnSmithModel.SiteName.Should().Be("Paris");
			johnSmithModel.TeamId.Should().Be(parisTeam.ToString());
			johnSmithModel.TeamName.Should().Be("ParisTeam");
			johnSmithModel.Name.Should().Be("123 - John Smith");

			var billGatesModel = agent.Single(x => x.PersonId == bill);
			billGatesModel.SiteId.Should().Be(london.ToString());
			billGatesModel.SiteName.Should().Be("London");
			billGatesModel.TeamId.Should().Be(londonTeam1.ToString());
			billGatesModel.TeamName.Should().Be("LondonTeam1");
			billGatesModel.Name.Should().Be("124 - Bill Gates");

			agent.Select(x => x.PersonId).Should().Not.Contain(ashley);
			agent.Select(x => x.PersonId).Should().Not.Contain(pierre);
		}

		[Test]
		public void ShouldGetAgentForSkillArea()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Now.Is("2017-01-20".Utc());

			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");
			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = john,
					FirstName = "John",
					LastName = "Smith",
					EmploymentNumber = "123",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Students",

				})
				.Has(new AgentStateReadModel
				{
					PersonId = bill,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "124",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Students",

				})
				.WithPersonSkill(john, skill1)
				.WithPersonSkill(bill, skill2);

			var viewModel = Target.For(new ViewModelFilter { SkillIds = new[] { skill1, skill2 } });
			var person1 = viewModel.States.Single(p => p.PersonId == john);
			person1.SiteId.Should().Be(siteId.ToString());
			person1.SiteName.Should().Be("London");
			person1.TeamId.Should().Be(teamId.ToString());
			person1.TeamName.Should().Be("Students");
			person1.Name.Should().Be("123 - John Smith");
			var person2 = viewModel.States.Single(p => p.PersonId == bill);
			person2.SiteId.Should().Be(siteId.ToString());
			person2.SiteName.Should().Be("London");
			person2.TeamId.Should().Be(teamId.ToString());
			person2.TeamName.Should().Be("Students");
			person2.Name.Should().Be("124 - Bill Gates");
		}

	}
}