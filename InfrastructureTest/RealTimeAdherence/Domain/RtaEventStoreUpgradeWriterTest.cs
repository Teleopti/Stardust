using System;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[UnitOfWorkTest]
	public class RtaEventStoreUpgradeWriterTest
	{
		public IRtaEventStore Events;
		public IRtaEventStoreUpgradeWriter Target;
		public IRtaEventStoreTester Tester;

		[Test]
		public void ShouldLoadForUpgrade()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			var result = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1);

			result.Single().Event.Should().Be.OfType<PersonStateChangedEvent>();
		}

		[Test]
		public void ShouldLoadBatch()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			var result = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 2);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadVersion1()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			Events.Add(new PersonRuleChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);

			var result = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 2);

			result.Single().Event.Should().Be.OfType<PersonStateChangedEvent>();
		}

		[Test]
		public void ShouldUpgrade()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			var @event = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1).Single();

			Target.Upgrade(@event, RtaEventStoreVersion.StoreVersion);

			Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1).Count().Should().Be(0);
		}

		[Test]
		public void ShouldUpgradeOneEvent()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			var events = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1);
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);

			Target.Upgrade(events.First(), RtaEventStoreVersion.StoreVersion);

			Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1).Count().Should().Be(1);
		}

		[Test]
		public void ShouldUpgradeEventData()
		{
			Events.Add(new PersonStateChangedEvent(), DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			var @event = Target.LoadForUpgrade(RtaEventStoreVersion.WithoutBelongsToDate, 1).Single();
			(@event.Event as PersonStateChangedEvent).BelongsToDate = "2018-10-31".Date();

			Target.Upgrade(@event, RtaEventStoreVersion.StoreVersion);

			Tester.LoadAllForTest().Cast<PersonStateChangedEvent>().Single().BelongsToDate
				.Should().Be("2018-10-31".Date());
		}
	}
}