using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreReadTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Events;
		public WithUnitOfWork WithUnitOfWork;
		public ConcurrencyRunner Run;

		[Test]
		public void ShouldLoadEvent()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2018-03-06 08:00".Utc()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-03-06 08:00 - 2018-03-06 10:00".Period()));

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

			var actual = WithUnitOfWork.Get(() => Events.Load(person1, new DateTimePeriod("2018-03-06 08:00".Utc(), "2018-03-06 09:00".Utc())));

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

			var actual = WithUnitOfWork.Get(() => Events.Load(person1, new DateTimePeriod("2018-03-06 07:00".Utc(), "2018-03-06 08:00".Utc())));

			actual.Cast<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2018-03-06 08:00".Utc());
		}
//
//		[Test]
//		public void ShouldOrderByStartTime()
//		{
//			var personId = Guid.NewGuid();
//			Publisher.Publish(
//				new PersonStateChangedEvent
//				{
//					PersonId = personId,
//					Timestamp = "2018-03-06 10:00".Utc()
//				},
//				new PersonStateChangedEvent
//				{
//					PersonId = personId,
//					Timestamp = "2018-03-06 08:00".Utc()
//				});
//
//			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-03-06 08:00 - 2018-03-06 10:00".Period()));
//
//			actual.Cast<PersonStateChangedEvent>()
//				.Select(x => x.Timestamp)
//				.Should().Have.SameSequenceAs("2018-03-06 08:00".Utc(), "2018-03-06 10:00".Utc());
//		}
//
//		[Test]
//		public void ShouldOrderByOrderOfOccurance()
//		{
//			var persons = Enumerable.Range(0, 3).Select(x => Guid.NewGuid()).ToArray();
//			var stateTimes = Enumerable.Range(0, 30)
//			var states = Enumerable.Range(0, 100).Select(x => )
//			var times = Enumerable.Range(0, 30)
//				.Select(x => $"2018-04-24 09:00".Utc().AddSeconds(x))
//				.Randomize()
//				.ToArray();
//			times.ForEach(t =>
//			{
//				Publisher.Publish(
//					new PeriodApprovedAsInAdherenceEvent
//					{
//						PersonId = personId,
//						StartTime = t,
//						EndTime = t.AddSeconds(1)
//					});
//			});
//
//			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-04-24 08:00 - 2018-04-24 23:00".Period()));
//
//			actual.Cast<PeriodApprovedAsInAdherenceEvent>()
//				.Select(x => x.StartTime)
//				.Should().Have.SameSequenceAs(times);
////		}
//
//		[Test]
//		public void ShouldOrderByOrderOfOccurance()
//		{
//			var personId = Guid.NewGuid();
//			var times = Enumerable.Range(0, 10)
//				.Select(x => $"2018-04-24 09:00".Utc().AddSeconds(x)).ToArray();
//
//			times.ForEach(t =>
//			{
//				Publisher.Publish(
//					new PeriodApprovedAsInAdherenceEvent
//					{
//						PersonId = personId,
//						StartTime = t,
//						EndTime = t.AddSeconds(1)
//					});
//			});
//			WithUnitOfWork.Do(uow =>
//			{
//				//TestReader.PutFirstLast();
////				var session = uow.Current().FetchSession();
////				session.CreateSQLQuery("set identity_insert rta.Events ON").ExecuteUpdate();
////				session.CreateSQLQuery("UPDATE [rta].[Events] SET Id = 1000 WHERE Id = 1").ExecuteUpdate();
////				session.CreateSQLQuery("set identity_insert rta.Events OFF").ExecuteUpdate();
//			});
//
//			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-04-24 08:00 - 2018-04-24 23:00".Period()));
//
//			var firstIsLast = times.Skip(1).Concat(times.First().AsArray()).ToArray();
//			actual.Cast<PeriodApprovedAsInAdherenceEvent>()
//				.Select(x => x.StartTime)
//				.Should().Have.SameSequenceAs(firstIsLast);
//		}
//
//		[Test]
//		public void ShouldOrderByOrderOfOccurance()
//		{
//			var persons = Enumerable.Range(0, 50)
//				.Select(x => Guid.NewGuid())
//				.ToArray();
//			var stateTimes = Enumerable.Range(0, 2000)
//				.Select(x => $"2018-04-24 09:00".Utc().AddSeconds(x))
//				.ToArray();
//			var approvePerson = persons.Second();
//			var approveTimes = stateTimes.Reverse().ToArray();
////.Take(10).ToArray();
//			Run.InParallel(() =>
//			{
//				stateTimes.ForEach(s =>
//				{
//					persons.ForEach(p =>
//					{
//						Publisher.Publish(
//							new PersonStateChangedEvent
//							{
//								PersonId = p,
//								Timestamp = s
//							});
//					});
//				});
//			});
//			Run.InParallel(() =>
//			{
//				approveTimes.ForEach(t =>
//				{
//					Publisher.Publish(
//						new PeriodApprovedAsInAdherenceEvent
//						{
//							PersonId = approvePerson,
//							StartTime = t,
//							EndTime = t.AddSeconds(1)
//						});
//				});
//			});
//			Run.Wait();
//
//			var loadPeriod = new DateTimePeriod(approveTimes.First().AddHours(-1), approveTimes.Last().AddHours(1));
//			100.Times(() =>
//			{
//				var actual = WithUnitOfWork.Get(() => Events.Load(approvePerson, loadPeriod));
//				actual.OfType<PeriodApprovedAsInAdherenceEvent>()
//					.Select(x => x.StartTime)
//					.Should().Have.SameSequenceAs(approveTimes);
//			});
//		}
//		

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

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-04-24 08:00 - 2018-04-24 23:00".Period()));

			var firstIsLast = times.Skip(1).Concat(times.First().AsArray()).ToArray();
			actual.Cast<PeriodApprovedAsInAdherenceEvent>()
				.Select(x => x.StartTime)
				.Should().Have.SameSequenceAs(firstIsLast);
		}
		
		[Test]
		public void ShouldLoadFirstTimeWithZero()
		{
			var events = WithUnitOfWork.Get(() => Events.LoadFrom(0));

			events.MaxId.Should().Be.EqualTo(0);			
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

			var events = WithUnitOfWork.Get(() => Events.LoadFrom(0));

			events.MaxId.Should().Be.GreaterThan(0);			
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

			var events = WithUnitOfWork.Get(() => Events.LoadFrom(0));

			events.MaxId.Should().Be.GreaterThan(1);			
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

			var events = WithUnitOfWork.Get(() => Events.LoadFrom(0)).Events;
			
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
			var events = WithUnitOfWork.Get(() => Events.LoadFrom(0));
			var maxId = events.MaxId;
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				Timestamp = "2018-03-06 09:00".Utc()
			});
			
			var latestEvents = WithUnitOfWork.Get(() => Events.LoadFrom(maxId));
			
			latestEvents.MaxId.Should().Be.GreaterThan(maxId);
			latestEvents.Events.Count().Should().Be(1);			
		}		
		
	}
}