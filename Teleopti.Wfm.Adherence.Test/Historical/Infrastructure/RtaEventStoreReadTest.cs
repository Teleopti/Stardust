using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreReadTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldLoadEvent()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-03-06 08:00".Utc(), "2018-03-06 10:00".Utc()));

			actual.Cast<PersonStateChangedEvent>().Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadEventsForPerson()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Publisher.Publish(
				new PersonStateChangedEvent
				{
					PersonId = person1,
					Timestamp = "2018-03-06 08:00".Utc(),
				},
				new PersonStateChangedEvent
				{
					PersonId = person2,
					Timestamp = "2018-03-06 08:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.Load(person1, "2018-03-06 08:00".Utc(), "2018-03-06 09:00".Utc()));

			actual.Cast<PersonStateChangedEvent>().Single().PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldLoadEventsForPeriod()
		{
			var person1 = Guid.NewGuid();
			Publisher.Publish(
				new PersonStateChangedEvent
				{
					PersonId = person1,
					Timestamp = "2018-03-06 08:00".Utc(),
				},
				new PersonStateChangedEvent
				{
					PersonId = person1,
					Timestamp = "2018-03-06 09:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.Load(person1, "2018-03-06 07:00".Utc(), "2018-03-06 08:00".Utc()));

			actual.Cast<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2018-03-06 08:00".Utc());
		}

		[Test]
		public void ShouldOrderByOrderOfOccurance_75683()
		{
			var personId = Guid.NewGuid();
			var times = Enumerable.Range(0, 10)
				.Select(x => $"2018-04-24 09:00".Utc().AddSeconds(x)).ToArray();
			times.ForEach(t =>
			{
				Publisher.Publish(
					new PeriodApprovedAsInAdherenceEvent
					{
						PersonId = personId,
						StartTime = t,
						EndTime = t.AddSeconds(1)
					});
			});
			WithUnitOfWork.Do(uow =>
			{
				// WARNING! BEAUTIFUL STUFF AHEAD!
				// #75683
				// At WFMRC1 sometimes the sql will not return events in Id order, but slightly random
				// We tried to repro in an infra test by adding alot of data and querying many times but could not
				// This will basically remove id identity contraint of the Id column to be able to reorder events
				// so that we can assert we return events ordered by Id 
				var session = uow.Current().FetchSession();
				session.CreateSQLQuery("ALTER TABLE [rta].[Events] DROP CONSTRAINT [PK_Events]").ExecuteUpdate();

				session.CreateSQLQuery("ALTER TABLE [rta].[Events] ADD Id2 int NULL").ExecuteUpdate();
				session.CreateSQLQuery("UPDATE [rta].[Events] SET Id2 = Id").ExecuteUpdate();
				session.CreateSQLQuery("ALTER TABLE [rta].[Events] DROP COLUMN Id").ExecuteUpdate();

				session.CreateSQLQuery("ALTER TABLE [rta].[Events] ADD Id int NULL").ExecuteUpdate();
				session.CreateSQLQuery("UPDATE [rta].[Events] SET Id = Id2").ExecuteUpdate();
				session.CreateSQLQuery("ALTER TABLE [rta].[Events] DROP COLUMN Id2").ExecuteUpdate();
			});
			WithUnitOfWork.Do(uow =>
			{
				var session = uow.Current().FetchSession();
				session.CreateSQLQuery("UPDATE [rta].[Events] SET Id = 1000 WHERE Id = 1").ExecuteUpdate();
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-04-24 08:00".Utc(), "2018-04-24 23:00".Utc()));

			var firstIsLast = times.Skip(1).Concat(times.First().AsArray()).ToArray();
			actual.Cast<PeriodApprovedAsInAdherenceEvent>()
				.Select(x => x.StartTime)
				.Should().Have.SameSequenceAs(firstIsLast);
		}

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