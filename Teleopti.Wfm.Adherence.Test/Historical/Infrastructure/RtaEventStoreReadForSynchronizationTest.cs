using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreReadForSynchronizationTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldLoadFirstTimeWithZero()
		{
			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0));

			events.ToId.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadEventFrom()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0));

			events.ToId.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldLoadEventFrom2()
		{
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 08:00".Utc()
			});

			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0));

			events.ToId.Should().Be.GreaterThan(1);
		}

		[Test]
		public void ShouldLoadPersonStateChangedEvent()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0)).Events;

			events.Cast<PersonStateChangedEvent>().Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadFromEvent()
		{
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 08:00".Utc()
			});
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 08:00".Utc()
			});
			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0));
			var maxId = events.ToId;
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 09:00".Utc()
			});

			var latestEvents = WithUnitOfWork.Get(() => Events.LoadForSynchronization(maxId));

			latestEvents.ToId.Should().Be.GreaterThan(maxId);
			latestEvents.Events.Count().Should().Be(1);
		}

		[Test]
		public void ShouldSetToPreviousMaxIfNoNewEvents()
		{
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-10-17 08:00".Utc()
			});
			var events = WithUnitOfWork.Get(() => Events.LoadForSynchronization(0));
			var previousMaxId = events.ToId;

			var latestEvents = WithUnitOfWork.Get(() => Events.LoadForSynchronization(previousMaxId));

			latestEvents.ToId.Should().Be(previousMaxId);
		}
	}
}