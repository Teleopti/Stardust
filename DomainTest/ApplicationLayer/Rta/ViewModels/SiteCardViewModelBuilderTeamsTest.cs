using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
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
	public class SiteCardViewModelBuilderTeamsTest : ISetup
	{
		public SiteCardViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeSiteRepository Sites;
		public FakeNumberOfAgentsInSiteReader AgentsInSite;
		public MutableNow Now;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldIncludeTeamsOfGivenSites()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId);

			var viewModel = Target.Build(null, new[] { siteId }).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Single().Id.Should().Be(teamId);
		}

		[Test]
		public void ShouldOnlyIncludeTeamsOnForGivenSites()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId);

			var viewModel = Target.Build(null, null).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeTeamProperties()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId, "green")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId,
					TeamName = "green",
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId);

			var viewModel = Target.Build(null, new[] { siteId }).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Single().Id.Should().Be(teamId);
			viewModel.Teams.Single().SiteId.Should().Be(siteId);
			viewModel.Teams.Single().Name.Should().Be("green");
			viewModel.Teams.Single().AgentsCount.Should().Be(1);
			viewModel.Teams.Single().InAlarmCount.Should().Be(1);
			viewModel.Teams.Single().Color.Should().Be("danger");
		}


	}
}