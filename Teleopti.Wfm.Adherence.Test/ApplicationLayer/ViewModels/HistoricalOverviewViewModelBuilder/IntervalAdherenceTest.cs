using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class BuildIntervalAdherenceTest
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldDisplayEmptyIntervalAdherence()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent();

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldDisplayIntervalAdherence()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 08:00", "2018-08-31 17:00")
				.WithHistoricalStateChange("2018-08-31 08:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2018-09-01 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(100);
		}

		[Test]
		public void ShouldDisplayCalculatedIntervalAdherence()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 20:00")
				.WithHistoricalStateChange("2018-08-31 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithAssignment("2018-09-01")
				.WithAssignedActivity("2018-09-01 10:00", "2018-09-01 20:00")
				.WithHistoricalStateChange("2018-09-01 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithHistoricalStateChange("2018-09-01 15:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(75);
		}

		[Test]
		public void ShouldDisplayFiftyPercentCalculatedIntervalAdherence()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 14:00")
				.WithHistoricalStateChange("2018-08-31 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithHistoricalStateChange("2018-08-31 11:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out)
				.WithAssignment("2018-09-01")
				.WithAssignedActivity("2018-09-01 10:00", "2018-09-01 20:00")
				.WithHistoricalStateChange("2018-09-01 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithHistoricalStateChange("2018-09-01 16:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(50);
		}

		[Test]
		public void ShouldDisplayCalculatedIntervalAdherenceForShortShift()
		{
			Now.Is("2018-08-31 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-31")
				.WithAssignedActivity("2018-08-31 10:00", "2018-08-31 10:30")
				.WithHistoricalStateChange("2018-08-31 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2018-09-02 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().IntervalAdherence.Should().Be(100);
		}
	}
}