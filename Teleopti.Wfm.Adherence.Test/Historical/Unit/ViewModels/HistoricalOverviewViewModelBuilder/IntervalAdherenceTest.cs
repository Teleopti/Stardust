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
	public class BuildIntervalAdherenceTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldDisplayEmpty()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-31 08:00", Adherence.Configuration.Adherence.Neutral);
			Now.Is("2018-09-01 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldDisplayOneHundredPercent()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 08:00", "2018-08-31 17:00")
				.WithHistoricalStateChange("2018-08-31 08:00", Adherence.Configuration.Adherence.In);
			Now.Is("2018-09-01 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(100);
		}		
		

		[Test]
		public void ShouldDisplaySeventyFivePercent()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 20:00")
				.WithHistoricalStateChange("2018-08-31 10:00", Adherence.Configuration.Adherence.In)
				.WithAssignment("2018-09-01")
				.WithAssignedActivity("2018-09-01 10:00", "2018-09-01 20:00")
				.WithHistoricalStateChange("2018-09-01 10:00", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-09-01 15:00", Adherence.Configuration.Adherence.Out);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(75);
		}

		[Test]
		public void ShouldDisplayFiftyPercent()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 14:00")
				.WithHistoricalStateChange("2018-08-31 10:00", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-08-31 11:00", Adherence.Configuration.Adherence.Out)
				.WithAssignment("2018-09-01")
				.WithAssignedActivity("2018-09-01 10:00", "2018-09-01 20:00")
				.WithHistoricalStateChange("2018-09-01 10:00", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-09-01 16:00", Adherence.Configuration.Adherence.Out);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(50);
		}

		[Test]
		public void ShouldDisplayForShortShift()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 10:30")
				.WithHistoricalStateChange("2018-08-31 10:00", Adherence.Configuration.Adherence.In);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(100);
		}		
	
		[Test]
		public void ShouldDisplayWithNeutralAdherence()
		{
			Now.Is("2018-09-20 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-09-20")
				.WithAssignedActivity("2018-09-20 10:00", "2018-09-20 18:00")
				.WithHistoricalStateChange("2018-09-20 10:00", Adherence.Configuration.Adherence.In)
				.WithAssignment("2018-09-21")
				.WithAssignedActivity("2018-09-21 10:00", "2018-09-21 18:00")
				.WithHistoricalStateChange("2018-09-21 10:00", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2018-09-21 16:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-09-21 17:00", Adherence.Configuration.Adherence.In);
			Now.Is("2018-09-22 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(90);
		}
		
		[Test]
		public void ShouldDisplayWithAllDayNeutralAdherence()
		{
			Now.Is("2018-09-20 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-09-20")
				.WithAssignedActivity("2018-09-20 10:00", "2018-09-20 18:00")
				.WithHistoricalStateChange("2018-09-20 10:00", Adherence.Configuration.Adherence.Neutral);
			Now.Is("2018-09-21 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(null);
		}
		
		[Test]
		public void ShouldDisplayWithAllDayOutAdherence()
		{
			Now.Is("2018-09-20 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-09-20")
				.WithAssignedActivity("2018-09-20 10:00", "2018-09-20 18:00")
				.WithHistoricalStateChange("2018-09-20 10:00", Adherence.Configuration.Adherence.Out);
			Now.Is("2018-09-21 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(0);
		}		
	}
}