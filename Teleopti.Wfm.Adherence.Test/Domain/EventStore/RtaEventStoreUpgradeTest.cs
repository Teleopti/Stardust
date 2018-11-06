using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.Domain.EventStore
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class RtaEventStoreUpgradeTest
	{
		public FakeRtaEventStore Events;
		public FakeDatabase Database;
		public FakeKeyValueStorePersister KeyValues;
		public IRtaEventStoreUpgrader Target;

		[Test]
		public void ShouldUpdateDate()
		{
			var person = Guid.NewGuid();
			Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();

			Events.LoadFrom(0).Events
				.OfType<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-30".Date());
		}

		[Test]
		public void ShouldUpdateDate2()
		{
			var person = Guid.NewGuid();
			Events.Add(new PersonRuleChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();

			Events.LoadFrom(0).Events
				.OfType<PersonRuleChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-30".Date());
		}

		[Test]
		public void ShouldUpdateMultiple()
		{
			var person = Guid.NewGuid();
			Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-31 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();

			var events = Events.LoadFrom(0).Events;
			events.OfType<PersonStateChangedEvent>().First().BelongsToDate.Should().Be("2018-10-30".Date());
			events.OfType<PersonStateChangedEvent>().Last().BelongsToDate.Should().Be("2018-10-31".Date());
		}

		[Test]
		public void ShouldUpdateDateOfNightShift()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-30")
				.WithAssignedActivity("2018-10-30 23:00", "2018-10-31 07:00")
				;
			Events.Add(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = person,
				StartTime = "2018-10-31 06:00".Utc(),
				EndTime = "2018-10-31 07:00".Utc()
			}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();

			Events.LoadFrom(0).Events
				.OfType<PeriodApprovedAsInAdherenceEvent>().Single().BelongsToDate.Should().Be("2018-10-30".Date());
		}

		[Test]
		public void ShouldUpdateNotUpdated()
		{
			var person = Guid.NewGuid();
			Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();
			Events.Data.Select(x => x.Event).Cast<PersonStateChangedEvent>().Single().BelongsToDate = null;
			Target.Upgrade();

			Events.LoadFrom(0).Events
				.OfType<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be(null);
		}

		[Test]
		public void ShouldKeepExistingDates()
		{
			var person = Guid.NewGuid();
			Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			Events.Data.Select(x => x.Event).Cast<PersonStateChangedEvent>().Single().BelongsToDate = "2018-10-31".Date();

			Target.Upgrade();

			Events.LoadFrom(0).Events
				.OfType<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-31".Date());
		}

		[Test]
		public void ShouldNotUpgradeIfUpgraded()
		{
			KeyValues.Update("RtaEventStoreVersion", RtaEventStoreVersion.StoreVersion);
			Events.Add(new PersonStateChangedEvent {PersonId = Guid.NewGuid(), Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade();

			Events.LoadFrom(0).Events.OfType<PersonStateChangedEvent>()
				.Single().BelongsToDate.Should().Be(null);
		}

		[Test]
		public void ShouldFlagUpgraded()
		{
			Target.Upgrade();

			KeyValues.Get("RtaEventStoreVersion", 0).Should().Be(RtaEventStoreVersion.StoreVersion);
		}
	}
}