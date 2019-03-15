using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	public class AdherenceAdjustToNeutralTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeRtaEventStore Events;
		public IRtaEventStoreSynchronizer Synchronizer;

		[Test]
		public void ShouldDisplayWithAdjustedToNeutral()
		{
			Now.Is("2019-02-01 08:00");
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-01")
				.WithAssignedActivity("2019-02-01 10:00", "2019-02-01 14:00");
			History
				.StateChanged(person, "2019-02-01 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-01 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 14:00");
			Now.Is("2019-02-08 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(50);
		}

		[Test]
		public void ShouldCalculateCorrectlyWithMultipleAdjustedPeriods()
		{
			Now.Is("2019-02-01 08:00");
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-01")
				.WithAssignedActivity("2019-02-01 10:00", "2019-02-01 14:00");
			History
				.StateChanged(person, "2019-02-01 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-01 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 14:00");

			History
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 14:00");
			Now.Is("2019-02-08 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(50);
		}

		[Test]
		public void ShouldCalculateCorrectlyWithMultipleDifferentAdjustedPeriods()
		{
			Now.Is("2019-02-18 08:00");
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-18")
				.WithAssignedActivity("2019-02-18 10:00", "2019-02-18 14:00");
			History
				.StateChanged(person, "2019-02-18 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-18 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-18 12:00", "2019-02-18 14:00")
				.AdjustedAdherenceToNeutral("2019-02-18 11:00", "2019-02-18 13:00");
			Now.Is("2019-02-25 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateCorrectlyWithManyDifferentAdjustedPeriods()
		{
			Now.Is("2019-02-18 08:00");
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-18")
				.WithAssignedActivity("2019-02-18 10:00", "2019-02-18 16:00");
			History
				.StateChanged(person, "2019-02-18 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-18 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-18 12:00", "2019-02-18 14:00")
				.AdjustedAdherenceToNeutral("2019-02-18 11:00", "2019-02-18 13:00")
				.AdjustedAdherenceToNeutral("2019-02-18 14:00", "2019-02-18 16:00");
			Now.Is("2019-02-25 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(0);
		}

		[Test]
		public void ShouldSynchronizeWellWithAdjustedEventInSecondBatch()
		{
			Events.LoadForSynchronizationSize = 1;
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-12")
				.WithAssignedActivity("2019-02-12 10:00", "2019-02-12 14:00");
			History
				.StateChanged(person, "2019-02-12 10:00", Adherence.Configuration.Adherence.Out)
				.AdjustedAdherenceToNeutral("2019-02-12 12:00", "2019-02-12 14:00")
				;
			Now.Is("2019-02-19 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSynchronizeAdjustedAdherenceToNeutralForPreviousDay()
		{
			Events.LoadForSynchronizationSize = 1;
			Events.SynchronizeOnAdd = false;
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-01")
				.WithAssignedActivity("2019-02-01 10:00", "2019-02-01 14:00");
			History
				.StateChanged(person, "2019-02-01 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-01 11:00", Adherence.Configuration.Adherence.In);
			Synchronizer.Synchronize();
			History
				.StateChanged(person, "2019-02-02 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 14:00");
			Now.Is("2019-02-08 08:00");
			
			Synchronizer.Synchronize();

			var data = Target.Build(null, new[] {teamId}).First();			
			data.Agents.First().Days.First().Adherence.Should().Be(50);
		}
	}
}