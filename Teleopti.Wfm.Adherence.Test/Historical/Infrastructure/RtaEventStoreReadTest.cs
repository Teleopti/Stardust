using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States.Events;
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
				BelongsToDate = "2018-03-06".Date(),
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-03-06".Date()));

			actual.Cast<PersonStateChangedEvent>().Single().PersonId.Should().Be(personId);
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
						BelongsToDate = new DateOnly(t.Date),
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

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-04-24".Date()));

			var firstIsLast = times.Skip(1).Concat(times.First().AsArray()).ToArray();
			actual.Cast<PeriodApprovedAsInAdherenceEvent>()
				.Select(x => x.StartTime)
				.Should().Have.SameSequenceAs(firstIsLast);
		}

		[Test]
		public void ShouldLoadOfType()
		{
			var period = new DateTimePeriod("2019-02-14 08:00".Utc(), "2019-02-14 18:00".Utc());
			Publisher.Publish(new PeriodAdjustedToNeutralEvent
			{
				StartTime = "2019-02-14 08:00".Utc(),
				EndTime = "2019-02-14 18:00".Utc(),
			});

			var actual = WithUnitOfWork.Get(() => Events.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period));

			var @event = actual.Cast<PeriodAdjustedToNeutralEvent>().Single();
			@event.StartTime.Should().Be("2019-02-14 08:00".Utc());
			@event.EndTime.Should().Be("2019-02-14 18:00".Utc());
		}

		[Test]
		public void ShouldLoadOfTypeForPeriod()
		{
			var period = new DateTimePeriod("2019-02-28 08:00".Utc(), "2019-02-28 18:00".Utc());
			Publisher.Publish(new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-27 08:00".Utc(),
					EndTime = "2019-02-27 18:00".Utc(),
				},
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-28 08:00".Utc(),
					EndTime = "2019-02-28 18:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period));

			var @event = actual.Cast<PeriodAdjustedToNeutralEvent>().Single();
			@event.StartTime.Should().Be("2019-02-28 08:00".Utc());
			@event.EndTime.Should().Be("2019-02-28 18:00".Utc());
		}

		[Test]
		public void ShouldLoadOfTypeStartingBeforePeriod()
		{
			var period = new DateTimePeriod("2019-02-28 08:00".Utc(), "2019-02-28 18:00".Utc());
			Publisher.Publish(
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-28 07:00".Utc(),
					EndTime = "2019-02-28 18:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period));

			var @event = actual.Cast<PeriodAdjustedToNeutralEvent>().Single();
			@event.StartTime.Should().Be("2019-02-28 07:00".Utc());
			@event.EndTime.Should().Be("2019-02-28 18:00".Utc());
		}
		
		[Test]
		public void ShouldLoadOfTypeEndingAfterPeriod()
		{
			var period = new DateTimePeriod("2019-02-28 08:00".Utc(), "2019-02-28 18:00".Utc());
			Publisher.Publish(
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-28 08:00".Utc(),
					EndTime = "2019-02-28 19:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period));

			var @event = actual.Cast<PeriodAdjustedToNeutralEvent>().Single();
			@event.StartTime.Should().Be("2019-02-28 08:00".Utc());
			@event.EndTime.Should().Be("2019-02-28 19:00".Utc());
		}
		
		[Test]
		public void ShouldLoadOfTypeWithinPeriod()
		{
			var period = new DateTimePeriod("2019-02-28 08:00".Utc(), "2019-02-28 18:00".Utc());
			Publisher.Publish(
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-28 09:00".Utc(),
					EndTime = "2019-02-28 17:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period));

			var @event = actual.Cast<PeriodAdjustedToNeutralEvent>().Single();
			@event.StartTime.Should().Be("2019-02-28 09:00".Utc());
			@event.EndTime.Should().Be("2019-02-28 17:00".Utc());
		}

		[Test]
		public void ShouldFindEventOfType()
		{
			Publisher.Publish(
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-20 08:00".Utc(),
					EndTime = "2019-02-20 18:00".Utc(),
				});

			var actual = WithUnitOfWork.Get(() => Events.AnyEventsOfType<PeriodAdjustedToNeutralEvent>(0));
			
			actual.Should().Be(true);
		}
		
		[Test]
		public void ShouldFindEventOfTypeFromId()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(
				new PeriodAdjustedToNeutralEvent
				{
					StartTime = "2019-02-20 08:00".Utc(),
					EndTime = "2019-02-20 18:00".Utc(),
				},
				new PersonStateChangedEvent
				{
					PersonId = personId,
					BelongsToDate = "2019-02-20".Date(),
					Timestamp = "2019-02-20 08:00".Utc()
				});

			var eventOfTypeFromId0 = WithUnitOfWork.Get(() => Events.AnyEventsOfType<PeriodAdjustedToNeutralEvent>(0));
			var eventOfTypeFromId1 = WithUnitOfWork.Get(() => Events.AnyEventsOfType<PeriodAdjustedToNeutralEvent>(1));
			
			eventOfTypeFromId0.Should().Be(true);
			eventOfTypeFromId1.Should().Be(false);
		}
	}
}