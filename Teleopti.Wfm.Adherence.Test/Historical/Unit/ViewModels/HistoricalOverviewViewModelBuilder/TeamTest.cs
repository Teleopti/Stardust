using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class BuildTeamTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetEmptyModel()
		{
			Target.Build(null, null)
				.Should().Have.SameValuesAs(Enumerable.Empty<HistoricalOverviewTeamViewModel>());
		}

		[Test]
		public void ShouldGetEmptyModelForNoAgentsInTeam()
		{
			var teamId = Guid.NewGuid();
			Database
				.WithSite("Barcelona")
				.WithTeam(teamId, "Blue");

			Target.Build(null, new[] {teamId})
				.Should().Have.SameValuesAs(Enumerable.Empty<HistoricalOverviewTeamViewModel>());
		}

		[Test]
		public void ShouldDisplayTeamBarcelonaBlue()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithSite("Barcelona")
				.WithTeam(teamId, "Blue")
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId});

			data.Single().Name.Should().Be("Barcelona/Blue");
		}

		[Test]
		public void ShouldDisplayTeamDenverRed()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithSite("Denver")
				.WithTeam(teamId, "Red")
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");
			
			var data = Target.Build(null, new[] {teamId});

			data.Single().Name.Should().Be("Denver/Red");
		}

		[Test]
		public void ShouldDisplayTeamInSite()
		{
			Now.Is("2018-08-23 14:00");
			var siteId = Guid.NewGuid();
			Database
				.WithSite(siteId, "Barcelona")
				.WithTeam("Blue")
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(new[] {siteId}, null);

			data.Single().Name.Should().Be("Barcelona/Blue");
		}

		[Test]
		public void ShouldDisplayTeamAndTeamInSite()
		{
			Now.Is("2018-08-23 14:00");
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite("Denver")
				.WithTeam(teamId, "Red")
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00")
				.WithSite(siteId, "Barcelona")
				.WithTeam("Blue")
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");
			
			var data = Target.Build(new[] {siteId}, new[] {teamId});

			data.First().Name.Should().Be("Barcelona/Blue");
			data.Second().Name.Should().Be("Denver/Red");
		}
		
		[Test]
		public void ShouldNotDisplayTeamWithNoAgentData()
		{
			Now.Is("2018-08-23 14:00");
			var siteId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam("Blue")
				.WithAgent();
			Now.Is("2018-08-24 14:00");			
			
			Target.Build(new[] {siteId}, null)
				.Should().Have.SameValuesAs(Enumerable.Empty<HistoricalOverviewTeamViewModel>());
		}
		
		[Test]
		public void ShouldNotBuildForToday()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId});
			
			data.Should().Have.SameValuesAs(Enumerable.Empty<HistoricalOverviewTeamViewModel>());
		}	
		
		[Test]
		public void ShouldGetEmptyModelIfNoValidAgent()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithSite("Barcelona")
				.WithTeam(teamId, "Blue")
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");
            
			Target.Build(null, new[] {teamId})
				.Should().Have.SameValuesAs(Enumerable.Empty<HistoricalOverviewTeamViewModel>());
		}	
	}
}