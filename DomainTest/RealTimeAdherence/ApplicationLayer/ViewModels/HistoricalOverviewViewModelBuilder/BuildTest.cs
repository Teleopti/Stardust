using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class BuildTest
	{
		public Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
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
				.WithTeam(teamId, "Blue")
				.WithHistoricalStateChange("2018-08-23 14:00");

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

			var data = Target.Build(null, new[] {teamId});

			data.Single().Name.Should().Be("Denver/Red");
		}

		[Test]
		public void ShouldGetAgentId()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			var agentId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(agentId)
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Id.Should().Be(agentId);
		}

		[Test]
		public void ShouldDisplayAgentBob()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgentNameDisplayedAs("{LastName} {FirstName}")
				.WithAgent("Bob Anderson")
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Name.Should().Be("Anderson Bob");
		}

		[Test]
		public void ShouldDisplayAgentLucy()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgentNameDisplayedAs("{LastName} {FirstName}")
				.WithAgent("Lucy Stone")
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Name.Should().Be("Stone Lucy");
		}

		[Test]
		public void ShouldDisplaySevenDays()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count().Should().Be(7);
		}

		[Test]
		public void ShouldGetADateForEachOfTheSevenDays()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(x => x.Date != null).Should().Be(7);
		}

		[Test]
		public void ShouldGetCorrectDateForFirstDay()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Date.Should().Be("20180816");
		}

		[Test]
		public void ShouldGetSevenDaysSequentially()
		{
			Now.Is("2018-08-24 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Select(x => x.Date).Should().Have.SameValuesAs(new[] {"20180817", "20180818", "20180819", "20180820", "20180821", "20180822", "20180823"});
		}

		[Test]
		public void ShouldDisplayDateForSevenDaysSequentially()
		{
			Now.Is("2018-08-24 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Select(x => x.DisplayDate).Should().Have.SameValuesAs(new[] {"08/17", "08/18", "08/19", "08/20", "08/21", "08/22", "08/23"});
		}

		[Test]
		public void ShouldDisplayEmptyAdherenceForSevenDaysIfNotBeenWorking()
		{
			Now.Is("2018-08-24 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(a => a.Adherence == null).Should().Be(7);
		}

		[Test]
		public void ShouldDisplayFullAdherence()
		{
			Now.Is("2018-08-23 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-23")
				.WithAssignedActivity("2018-08-23 08:00", "2018-08-23 17:00")
				.WithHistoricalStateChange("2018-08-23 08:00", Adherence.In);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Last().Adherence.Should().Be("100");
		}

		[Test]
		public void ShouldDisplayCalculatedAdherence()
		{
			Now.Is("2018-08-17 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-17")
				.WithAssignedActivity("2018-08-17 10:00", "2018-08-17 20:00")
				.WithHistoricalStateChange("2018-08-17 10:00", Adherence.In)
				.WithHistoricalStateChange("2018-08-17 15:00", Adherence.Out);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be("50");
		}

		[Test]
		public void ShouldNotDisplayLateForWork()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent();

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().WasLateForWork.Should().Be(false);
		}

		[Test]
		public void ShouldDisplayLateForWork()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Last().WasLateForWork.Should().Be(true);
		}

		[Test]
		public void ShouldDisplayLateForWorkSumZero()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent();

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.Count.Should().Be(0);
		}

		[Test]
		public void ShouldDisplayLateForWorkSum()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.Count.Should().Be(1);
		}

		[Test]
		public void ShouldDisplayLateForkWorkSumInMinutes()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent();

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.TotalMinutes.Should().Be(0);
		}

		[Test]
		public void ShouldDisplayLateForkWorkSum60Minutes()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.TotalMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldDisplayTeamInSite()
		{
			Now.Is("2018-08-23 14:00");
			var siteId = Guid.NewGuid();

			Database
				.WithSite(siteId, "Barcelona")
				.WithTeam("Blue")
				.WithAgent();

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
				.WithSite(siteId, "Barcelona")
				.WithTeam("Blue")
				.WithAgent();

			var data = Target.Build(new[] {siteId}, new[] {teamId});

			data.First().Name.Should().Be("Barcelona/Blue");
			data.Second().Name.Should().Be("Denver/Red");
		}
	}
}