using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentViewModelBuilderTest : ISetup
	{
		public AgentViewModelBuilder Target;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeCommonAgentNameProvider CommonAgentNameProvider;
		public FakeDatabase Database;
        public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<UnPromiscuousFakeGroupingReadOnlyRepository>().For<FakeGroupingReadOnlyRepository, IGroupingReadOnlyRepository>();
		}

		[Test]
		public void ShouldGetAgentForSites()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(siteId, "bla")
				.WithTeam(teamId, "angel")
				.WithAgent(personId, "Bill Gates", 123)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;

			var agent = Target.For(new AgentStateFilter {SiteIds = new[] {siteId}}).Single();
			agent.Name.Should().Be("123 - Bill Gates");
			agent.PersonId.Should().Be(personId);
			agent.TeamId.Should().Be(teamId.ToString());
			agent.TeamName.Should().Be("angel");
			agent.SiteId.Should().Be(siteId.ToString());
			agent.SiteName.Should().Be("bla");
		}

		[Test]
		public void ShouldGetAgentForTeams()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(siteId, "bla")
				.WithTeam(teamId, "angel")
				.WithAgent(personId, "Bill Gates", 123)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;

			var agent = Target.For(new AgentStateFilter {TeamIds = new[] {teamId}}).Single();
			agent.Name.Should().Be("123 - Bill Gates");
			agent.PersonId.Should().Be(personId);
			agent.TeamId.Should().Be(teamId.ToString());
			agent.TeamName.Should().Be("angel");
			agent.SiteId.Should().Be(siteId.ToString());
			agent.SiteName.Should().Be("bla");
		}
		
		[Test]
		public void ShouldGetAgentForSkill()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(siteId, "bla")
				.WithTeam(teamId, "angel")
				.WithAgent(personId, "John Smith", 123)
				.WithSkill(skillId)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;
			
			var result = Target.For(new AgentStateFilter {SkillIds = new[] { skillId } }).Single();

			result.PersonId.Should().Be(personId);
			result.SiteId.Should().Be(siteId.ToString());
			result.SiteName.Should().Be("bla");
			result.TeamId.Should().Be(teamId.ToString());
			result.TeamName.Should().Be("angel");
			result.Name.Should().Be("123 - John Smith");
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
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(paris, "paris")
				.WithTeam(red, "red")
				.WithAgent(john, "John Smith", 123)
				.WithSite(london, "london")
				.WithTeam(students, "students")
				.WithAgent(bill, "Bill Gates", 124)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;

			var viewModel = Target.For(new AgentStateFilter { SiteIds = new[] { london }, TeamIds = new[] { red } });

			var johnVm = viewModel.Single(p => p.PersonId == john);
			johnVm.SiteId.Should().Be(paris.ToString());
			johnVm.SiteName.Should().Be("paris");
			johnVm.TeamId.Should().Be(red.ToString());
			johnVm.TeamName.Should().Be("red");
			johnVm.Name.Should().Be("123 - John Smith");

			var billVm = viewModel.Single(p => p.PersonId == bill);
			billVm.SiteId.Should().Be(london.ToString());
			billVm.SiteName.Should().Be("london");
			billVm.TeamId.Should().Be(students.ToString());
			billVm.TeamName.Should().Be("students");
			billVm.Name.Should().Be("124 - Bill Gates");

		}

		[Test]
		public void ShouldGetAgentForSkillAndTeam()
		{
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			var paris = Guid.NewGuid();
			var redId = Guid.NewGuid();
			var greenId = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(paris, "paris")
				.WithTeam(redId, "red")
				.WithAgent(john, "John Smith", 123)
				.WithSkill(skill)
				.WithTeam(greenId, "green")
				.WithAgent(bill, "Bill Gates", 124)
				.WithSkill(skill)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;

			var result = Target.For(new AgentStateFilter {TeamIds = new[] {greenId}, SkillIds = new[] {skill}}).Single();

			result.PersonId.Should().Be(bill);
			result.SiteId.Should().Be(paris.ToString());
			result.SiteName.Should().Be("paris");
			result.TeamId.Should().Be(greenId.ToString());
			result.TeamName.Should().Be("green");
			result.Name.Should().Be("124 - Bill Gates");
		}

		[Test]
		public void ShouldGetAgentForSkillAndSite()
		{
			var paris = Guid.NewGuid();
			var red = Guid.NewGuid();
			var london = Guid.NewGuid();
			var students = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var john = Guid.NewGuid();
			var bill = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithSite(paris, "paris")
				.WithTeam(red, "red")
				.WithAgent(john, "John Smith", 123)
				.WithSkill(skill)
				.WithSite(london, "london")
				.WithTeam(students, "students")
				.WithAgent(bill, "Bill Gates", 124)
				.WithSkill(skill)
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")
				;

			var result = Target.For(new AgentStateFilter {SiteIds = new[] {london}, SkillIds = new[] {skill}}).Single();

			result.PersonId.Should().Be(bill);
			result.SiteId.Should().Be(london.ToString());
			result.SiteName.Should().Be("london");
			result.TeamId.Should().Be(students.ToString());
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
				.WithSite(siteId, "Paris")
				.WithTeam(teamId, "Angel")
				.WithAgent(personId1, "John Smith", teamId, siteId)
				.WithSkill(skill1)
				.WithAgent(personId2, "Ashley Andeen", teamId, siteId)
				.WithSkill(skill2)
				.WithAgentNameDisplayedAs("{FirstName} {LastName}")
				;

			var viewModel = Target.For(new AgentStateFilter {SkillIds = new[] { skill1, skill2 } });
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

		[Test]
		public void ShouldGetAgentForSkillSiteAndTeam()
		{
			var paris = Guid.NewGuid();
			var parisTeam = Guid.NewGuid();
			var london = Guid.NewGuid();
			var londonTeam1 = Guid.NewGuid();
			var londonTeam2 = Guid.NewGuid();
			var support = Guid.NewGuid();
			var sales = Guid.NewGuid();
			var johnSmith = Guid.NewGuid();
			var billGates = Guid.NewGuid();
			var pierreBaldi = Guid.NewGuid();
			var ashleyAndeen = Guid.NewGuid();
			Now.Is("2016-11-09".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}")

				.WithSite(paris, "paris")
				.WithTeam(parisTeam, "parisTeam")
				.WithAgent(johnSmith, "John Smith", 123)
				.WithSkill(support)

				.WithSite(london, "london")
				.WithTeam(londonTeam1, "londonTeam1")
				.WithAgent(billGates, "Bill Gates", 124)
				.WithSkill(support)


				.WithSite(london, "london")
				.WithTeam(londonTeam1, "londonTeam1")
				.WithAgent(pierreBaldi, "Pierre Baldi", 125)
				.WithSkill(sales)

				.WithTeam(londonTeam2, "londonTeam2")
				.WithAgent(ashleyAndeen, "Ashley Andeen", 126)
				.WithSkill(support)
				;

			var result = Target.For(new AgentStateFilter { SiteIds = new[] { paris }, TeamIds = new [] {londonTeam1}, SkillIds = new[] { support } });

			var johnSmithModel = result.Single(x => x.PersonId == johnSmith);
			johnSmithModel.SiteId.Should().Be(paris.ToString());
			johnSmithModel.SiteName.Should().Be("paris");
			johnSmithModel.TeamId.Should().Be(parisTeam.ToString());
			johnSmithModel.TeamName.Should().Be("parisTeam");
			johnSmithModel.Name.Should().Be("123 - John Smith");

			var billGatesModel = result.Single(x => x.PersonId == billGates);
			billGatesModel.SiteId.Should().Be(london.ToString());
			billGatesModel.SiteName.Should().Be("london");
			billGatesModel.TeamId.Should().Be(londonTeam1.ToString());
			billGatesModel.TeamName.Should().Be("londonTeam1");
			billGatesModel.Name.Should().Be("124 - Bill Gates");

			result.Select(x => x.PersonId).Should().Not.Contain(ashleyAndeen);
			result.Select(x => x.PersonId).Should().Not.Contain(pierreBaldi);
		}
	}
}
