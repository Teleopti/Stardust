using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	public class AdherenceAdjustToNeutralTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeHistoricalOverviewReadModelPersister ReadModels;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeRtaEventStore Events;

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
		public void ShouldNotUpdateReadModelWithEmptyPersonId()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person);

			History
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 14:00");

			ReadModels.Read(new[] {Guid.Empty})
				.Where(x => x.PersonId == Guid.Empty)
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldUpdateReadModelWithoutBelongsToDate()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person);

			Events.Add(
				new PersonStateChangedEvent
				{
					PersonId = person,
					Timestamp = "2018-10-30 08:00".Utc()
				},
				DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate
			);

			ReadModels.Read(person.AsArray())
				.Single().Date
				.Should().Be(new DateOnly("2018-10-30".Utc()));
		}
	}
}