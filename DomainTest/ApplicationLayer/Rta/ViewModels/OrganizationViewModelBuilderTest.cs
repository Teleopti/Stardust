using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class OrganizationViewModelBuilderTest : ISetup
	{
		public OrganizationViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldBuild()
		{
			Database
				.WithSite(null, "Paris")
				.WithTeam(null,"Team");

			var result = Target.Build();

			result.Single().Name.Should().Be("Paris");
		}
		
		[Test]
		public void ShouldSortByName()
		{
			Database
				.WithSite(null, "C")
				.WithTeam(null, "Team 1")
				.WithSite(null, "B")
				.WithTeam(null, "Team 2")
				.WithSite(null, "A")
				.WithTeam(null, "Team 3");

			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "B", "C"});
		}

		[Test]
		public void ShouldSortSwedishName()
		{
			Database
				.WithSite(null, "Ä")
				.WithTeam(null, "Team 1")
				.WithSite(null, "A")
				.WithTeam(null, "Team 2")
				.WithSite(null, "Å")
				.WithTeam(null, "Team 3");

			UiCulture.IsSwedish();
			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "Å", "Ä"});
		}
		
		[Test]
		public void ShouldNotReturnEmptySite()
		{
			var siteId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithAgent(personId)
				.WithSkill(skillId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId
				})
				.WithSite();

			Target.BuildForSkills(new[] {skillId}).Single()
				.Id.Should().Be(siteId);
		}

		[Test]
		public void ShouldBuildForOrganization()
		{
			var london = Guid.NewGuid();
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithSite(london, "London")
				.WithTeam(team1,"Team1")
				.WithAgent(person1)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = person1,
					SiteId = london,
					TeamId = team1
				})
				.WithTeam(team2, "Team2")
				.WithAgent(person2)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = person2,
					SiteId = london,
					TeamId = team2
				});
			
			var org = Target.Build().Single();
			
			org.Id.Should().Be(london);
			org.Name.Should().Be("London");
			org.Teams.Select(t => t.Id).Should().Have.SameValuesAs(team1, team2);
			org.Teams.Select(t => t.Name).Should().Have.SameValuesAs("Team1", "Team2");
		}

		[Test]
		public void ShouldBuildForOrganizationExcludeSiteIfNoTeam()
		{
			var london = Guid.NewGuid();
			var excludedSite = Guid.NewGuid();
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			Database
				.WithSite(london, "London")
				.WithTeam(team1, "Team1")
				.WithTeam(team2, "Team2")
				.WithSite(excludedSite);

			var org = Target.Build().Single();

			org.Id.Should().Be(london);
			org.Name.Should().Be("London");
			org.Teams.Select(t => t.Id).Should().Have.SameValuesAs(team1, team2);
			org.Teams.Select(t => t.Name).Should().Have.SameValuesAs("Team1", "Team2");
		}
		
		[Test]
		public void ShouldBuildForOrganizationWithSkills()
		{
			var london = Guid.NewGuid();
			var wrongSite = Guid.NewGuid();
			var team = Guid.NewGuid();
			var wrongTeam = Guid.NewGuid();
			var londonWrongTeam = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var wrongSkillId = Guid.NewGuid();
			Database
				.WithSite(london, "London")
				.WithTeam(team, "Team")
				.WithAgent()
				.WithSkill(skillId)
				.WithAgent()
				.WithSkill(wrongSkillId)
				.WithTeam(londonWrongTeam, "LondonWrongTeam")
				.WithAgent()
				.WithSkill(wrongSkillId)
				.WithSite(wrongSite, "WrongSite")
				.WithTeam(wrongTeam, "WrongTeam")
				.WithAgent()
				;

			var org = Target.BuildForSkills(new[] { skillId }).Single();

			org.Id.Should().Be(london);
			org.Name.Should().Be("London");
			org.Teams.Select(t => t.Id).Should().Have.SameValuesAs(team);
			org.Teams.Select(t => t.Name).Should().Have.SameValuesAs("Team");

		}

	}
}