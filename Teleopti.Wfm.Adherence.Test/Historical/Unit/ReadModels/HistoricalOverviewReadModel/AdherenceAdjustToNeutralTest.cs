using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ReadModels.HistoricalOverviewReadModel
{
	[DomainTest]
	public class AdherenceAdjustToNeutralTest
	{
		public FakeHistoricalOverviewReadModelPersister ReadModels;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeRtaEventStore Events;
		public IRtaEventStoreSynchronizer Synchronizer;

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

		[Test]
		public void ShouldOnlyUpdateReadModelOncePerAgentAndDay()
		{
			Events.SynchronizeOnAdd = false;
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			History
				.StateChanged(person1, "2019-03-15 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person2, "2019-03-15 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person1, "2019-03-15 09:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person2, "2019-03-15 09:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person1, "2019-03-16 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person2, "2019-03-16 08:00", Adherence.Configuration.Adherence.Out)
				;
			
			Synchronizer.Synchronize();

			ReadModels.UpsertCount.Should().Be(4);
		}
	}
}