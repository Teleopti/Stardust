using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
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
	public class TeamViewModelBuilderTest : ISetup
	{
		public TeamViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeUserUiCulture UiCulture;
		public FakeNumberOfAgentsInTeamReader AgentsInTeam;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldBuild()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("Green");

			var result = Target.Build(site);

			result.Single().Name.Should().Be("Green");
		}

		[Test]
		public void ShouldSortByName()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("C")
				.WithTeam("B")
				.WithTeam("A");

			var result = Target.Build(site);

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] { "A", "B", "C" });
		}

		[Test]
		public void ShouldSortSwedishName()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("Ä")
				.WithTeam("A")
				.WithTeam("Ĺ");
			UiCulture.IsSwedish();

			var result = Target.Build(site);

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] { "A", "Ĺ", "Ä" });
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId, "Team Red")
				.WithAgent();

			var result = Target.Build(siteId).Single();

			result.Name.Should().Be("Team Red");
			result.Id.Should().Be(teamId);
			result.NumberOfAgents.Should().Be(1);
		}

		[Test]
		public void ShouldGetNumberOfAgentsForSkill()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var wrongPerson = Guid.NewGuid();
			var wrongSkill = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId, "Team Red")
				.WithAgent()
				.WithSkill(skillId)
				.WithAgent(wrongPerson)
				.WithSkill(wrongSkill);
		

			var result = Target.ForSkills(siteId, new[] { skillId }).Single();

			result.Name.Should().Be("Team Red");
			result.Id.Should().Be(teamId);
			result.NumberOfAgents.Should().Be(1);
		}

		[Test]
		public void ShouldNotReturnEmptyTeam()
		{
			var siteId = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId1)
				.WithAgent()
				.WithSkill(skillId)
				.WithTeam(teamId2);

			Target.ForSkills(siteId, new[] { skillId }).Single()
				.Id.Should().Be(teamId1);
		}
	}
}