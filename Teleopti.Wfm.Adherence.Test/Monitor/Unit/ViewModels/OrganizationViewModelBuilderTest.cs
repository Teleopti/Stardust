using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels
{
	[DomainTest]
	public class OrganizationViewModelBuilderTest : IIsolateSystem
	{
		public OrganizationViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeUserUiCulture UiCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldBuild()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithSite(siteId, "Paris")
				.WithTeam(teamId, "Team")
				.WithAgent(personId)
				.WithAgentState(new AgentStateReadModel
				{
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = siteId,
					SiteName = "Paris",
					TeamId = teamId,
					TeamName = "Team",
					PersonId = personId				
				});

			var result = Target.Build();

			result.Single().Name.Should().Be("Paris");
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
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					PersonId = person1,
					SiteId = london,
					SiteName = "London",
					TeamId = team1,
					TeamName = "Team1"
				})
				.WithTeam(team2, "Team2")
				.WithAgent(person2)
				.WithAgentState(new AgentStateReadModel
				{
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					PersonId = person2,
					SiteId = london,
					SiteName = "London",
					TeamId = team2,
					TeamName = "Team2"
				});
			
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
			var team = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var expectedPerson = Guid.NewGuid();
			

			var wrongSkillId = Guid.NewGuid();
			var wrongSkillPerson = Guid.NewGuid();
			
			var londonWrongTeam = Guid.NewGuid();
			var londonWrongTeamPerson = Guid.NewGuid();

			var wrongSite = Guid.NewGuid();
			var wrongTeam = Guid.NewGuid();
			var wrongSitePerson = Guid.NewGuid();			


			Database
				.WithSite(london, "London")
				.WithTeam(team, "Team")
				.WithAgent(expectedPerson)
				.WithSkill(skill)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = expectedPerson,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = london,
					SiteName = "London",
					TeamId = team,
					TeamName = "Team"
				})

				.WithAgent(wrongSkillPerson)
				.WithSkill(wrongSkillId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = wrongSkillPerson,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = london,
					SiteName = "London",
					TeamId = team,
					TeamName = "Team"
				})

				.WithTeam(londonWrongTeam, "LondonWrongTeam")
				.WithAgent(londonWrongTeamPerson)
				.WithSkill(wrongSkillId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = londonWrongTeamPerson,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = london,
					SiteName = "London",
					TeamId = londonWrongTeam,
					TeamName = "LondonWrongTeam"
				})

				.WithSite(wrongSite, "WrongSite")
				.WithTeam(wrongTeam, "WrongTeam")
				.WithAgent(wrongSitePerson)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = wrongSitePerson,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = wrongSite,
					SiteName = "WrongSite",
					TeamId = wrongTeam,
					TeamName = "WrongTeam"
				})
				;

			var org = Target.BuildForSkills(new[] { skill }).Single();

			org.Id.Should().Be(london);
			org.Name.Should().Be("London");
			org.Teams.Select(t => t.Id).Should().Have.SameValuesAs(team);
			org.Teams.Select(t => t.Name).Should().Have.SameValuesAs("Team");

		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldOrderBySitesNameAccordingToSwedishName()
		{
			var businessUnitId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var siteId3 = Guid.NewGuid();
			var skill = Guid.NewGuid();

			Database
				.WithSite(siteId1, "Å")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId1,
					BusinessUnitId = businessUnitId,
					SiteId = siteId1,
					SiteName = "Å",
					TeamId = Guid.NewGuid()
				})
				.WithAgent(personId1)
				.WithSkill(skill)
				.WithSite(siteId2, "Ä")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = businessUnitId,
					SiteId = siteId2,
					SiteName = "Ä",
					TeamId = Guid.NewGuid()
				})
				.WithAgent(personId2)
				.WithSkill(skill)
				.WithSite(siteId3, "A")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = businessUnitId,
					SiteId = siteId3,
					SiteName = "A",
					TeamId = Guid.NewGuid()
				})
				.WithAgent(personId3)
				.WithSkill(skill);

			var result = Target.Build();
			result.Select(x => x.Id)
				.Should().Have.SameSequenceAs(new[]
				{
					siteId3,
					siteId1,
					siteId2
				});
			var resultForSkill = Target.BuildForSkills(new[] { skill }).Select(x => x.Id);
			result.Select(x => x.Id).Should().Have.SameSequenceAs(resultForSkill);
		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldOrderTeams()
		{
			var businessUnitId = Guid.NewGuid();
			
			var site = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var teamId3 = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var skill = Guid.NewGuid();

			Database
				.WithSite(site, "s")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId1,
					BusinessUnitId = businessUnitId,
					SiteId = site,
					SiteName = "s",
					TeamId = teamId1,
					TeamName = "Å"
				})
				.WithTeam(teamId1, "Å")
				.WithAgent(personId1)
				.WithSkill(skill)

				.WithTeam(teamId2, "Ö")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = businessUnitId,
					SiteId = site,
					SiteName = "s",
					TeamId = teamId2,
					TeamName = "Ö"
				})
				.WithAgent(personId2)
				.WithSkill(skill)


				.WithTeam(teamId3, "Ä")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = businessUnitId,
					SiteId = site,
					SiteName = "s",
					TeamId = teamId3,
					TeamName = "Ä"
				})
				.WithAgent(personId3)
				.WithSkill(skill)
				;

			var result = Target.Build().Single()
				.Teams.Select(x => x.Id);
			result
				.Should().Have.SameSequenceAs(new[]
				{
					teamId1,
					teamId3,
					teamId2
				});

			var resultForSkill = Target.BuildForSkills(new[] { skill }).Single().Teams.Select(x => x.Id);
			result.Should().Have.SameSequenceAs(resultForSkill);
		}
	}
}