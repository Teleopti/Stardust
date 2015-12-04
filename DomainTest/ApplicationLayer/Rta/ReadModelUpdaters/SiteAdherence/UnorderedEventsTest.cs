using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		[TestCaseSource(typeof(EventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));
			var siteId = events.OfType<PersonOutOfAdherenceEvent>().First().SiteId;
			Persister.Get(siteId).Count.Should().Be(2);
		}
		
		[Test]
		[Ignore]
		// ???
		public void ShouldHandleWhenPeronDeletedEventIsTheFirstEvent_IsItWorthToImplent_QuestionMark() 
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			var events = new IEvent[]
			{
					new PersonDeletedEvent
					{
						PersonId = personId,
						Timestamp = "2015-12-04 08:05".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-12-04 08:00".Utc()
					},
			};
			events.ForEach(e => Target.Handle((dynamic) e));

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldHandleOutOfSyncPersonDeletedEvents()
		{
			var person = Guid.NewGuid();
			var site = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent {PersonId = Guid.NewGuid(), SiteId = site, Timestamp = "2015-12-04 10:00".Utc()});

			Target.Handle(new PersonOutOfAdherenceEvent {PersonId = person, SiteId = site, Timestamp = "2015-12-04 10:05".Utc()});
			Target.Handle(new PersonDeletedEvent {PersonId = person, Timestamp = "2015-12-04 10:00".Utc()});

			Persister.Get(site).Count.Should().Be(1);
		}
	}

	public class EventsPermuationFactory
	{
		public static IEnumerable Permutations
		{
			get
			{
				var personId1 = Guid.NewGuid();
				var personId2 = Guid.NewGuid();
				var siteId = Guid.NewGuid();
				var events = new List<IEvent>
				{

					new PersonOutOfAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:00".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:04".Utc()
					},

					new PersonInAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					}
				};
				return getTestData(events);
			}
		}

		private static IEnumerable getTestData(IEnumerable<IEvent> events)
		{
			var permutations = events.Permutations();
			return from p in permutations
				   let name = (from pe in p select pe.GetType().Name).Aggregate((current, next) => current + ", " + next)
				   select new TestCaseData(p).SetName(name);
		}
	}
}