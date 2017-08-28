using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.HistoricalAdherence
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_EventPackagesOptimization_43924)]
	public class PackageTest
	{
		public FakeHistoricalAdherenceReadModelPersister AdherencePersister;
		public FakeHistoricalChangeReadModelPersister ChangePersister;
		public HistoricalAdherenceUpdaterWithPackages Target;
		public MutableNow Now;

		[Test]
		public void ShouldSubscribeToEvents()
		{
			var subscriptionsRegistrator = new SubscriptionRegistrator();

			Target.Subscribe(subscriptionsRegistrator);

			subscriptionsRegistrator.SubscribesTo(typeof(PersonInAdherenceEvent)).Should().Be(true);
			subscriptionsRegistrator.SubscribesTo(typeof(PersonOutOfAdherenceEvent)).Should().Be(true);
			subscriptionsRegistrator.SubscribesTo(typeof(PersonNeutralAdherenceEvent)).Should().Be(true);
		}

		[Test]
		public void ShouldPersist()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonOutOfAdherenceEvent {PersonId = personId, Timestamp = "2017-05-03 12:00".Utc()}
			});


			AdherencePersister.Read(personId, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
		}

		[Test]
		public void ShouldPersistForMultipleAgents()
		{
			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonOutOfAdherenceEvent {PersonId = personId, Timestamp = "2017-05-03 12:00".Utc()},
				new PersonOutOfAdherenceEvent {PersonId = personId2, Timestamp = "2017-05-03 12:00".Utc()}
			});

			AdherencePersister.Read(personId, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
			AdherencePersister.Read(personId2, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
		}


		[Test]
		public void ShouldPutOutOfAdherenceEndTime()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonOutOfAdherenceEvent
				{
					PersonId = personId,
					BelongsToDate = "2017-05-03".Date(),
					Timestamp = "2017-05-03 12:00".Utc()
				}
			});
			Target.Handle(new[]
			{
				new PersonInAdherenceEvent
				{
					PersonId = personId,
					BelongsToDate = "2017-05-03".Date(),
					Timestamp = "2017-05-03 12:15".Utc()
				}
			});

			AdherencePersister.Read(personId, "2017-05-03".Date()).Last().Timestamp.Should().Be("2017-05-03 12:15".Utc());
		}

		[Test]
		public void ShouldHandleNeutralAdherenceEvent()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new[]
			{
				new PersonNeutralAdherenceEvent
				{
					PersonId = personId,
					BelongsToDate = "2017-05-03".Date(),
					Timestamp = "2017-05-03 12:15".Utc()
				}
			});

			AdherencePersister.Read(personId, "2017-05-03".Date()).Last().Timestamp.Should().Be("2017-05-03 12:15".Utc());
		}
	}
}