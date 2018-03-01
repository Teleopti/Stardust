using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ReadModels.HistoricalAdherence
{
	[DomainTest]
	[TestFixture]
	public class PackageTest
	{
		public FakeHistoricalChangeReadModelPersister ChangePersister;
		public HistoricalChangeUpdater Target;
		public MutableNow Now;

		[Test]
		public void ShouldSubscribeToEvents()
		{
			var subscriptionsRegistrator = new SubscriptionRegistrator();

			Target.Subscribe(subscriptionsRegistrator);

			subscriptionsRegistrator.SubscribesTo(typeof(PersonStateChangedEvent)).Should().Be(true);
			subscriptionsRegistrator.SubscribesTo(typeof(PersonRuleChangedEvent)).Should().Be(true);
		}

		[Test]
		public void ShouldPersist()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonStateChangedEvent {PersonId = personId, Timestamp = "2017-05-03 12:00".Utc()}
			});


			ChangePersister.Read(personId, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
		}

		[Test]
		public void ShouldPersistForMultipleAgents()
		{
			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonStateChangedEvent {PersonId = personId, Timestamp = "2017-05-03 12:00".Utc()},
				new PersonStateChangedEvent {PersonId = personId2, Timestamp = "2017-05-03 12:00".Utc()}
			});

			ChangePersister.Read(personId, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
			ChangePersister.Read(personId2, "2017-05-03".Date()).Single().Timestamp.Should().Be("2017-05-03 12:00".Utc());
		}

		[Test]
		public void ShouldHandlePersonStateChangedEvent()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new[]
			{
				new PersonStateChangedEvent
				{
					PersonId = personId,
					BelongsToDate = "2017-05-03".Date(),
					Timestamp = "2017-05-03 12:15".Utc()
				}
			});

			ChangePersister.Read(personId, "2017-05-03".Date()).Last().Timestamp.Should().Be("2017-05-03 12:15".Utc());
		}
	}
}