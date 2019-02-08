using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	public class BuildAgentTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

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
			Now.Is("2018-08-24 14:00");

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
			Now.Is("2018-08-24 14:00");

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
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Name.Should().Be("Stone Lucy");
		}
		
		[Test]
		public void ShouldOnlyDisplayAgentsWithData()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgentNameDisplayedAs("{LastName} {FirstName}")
				.WithAgent("Lucy Stone")
				.WithHistoricalStateChange("2018-08-23 14:00")
				.WithAgent("Bob Anderson");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Name.Should().Be("Stone Lucy");
		}
	}
}