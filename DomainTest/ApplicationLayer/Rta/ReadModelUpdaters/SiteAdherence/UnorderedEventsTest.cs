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
	[ReadModelUpdaterTest]
	[TestFixture]
	public class UnorderedEventsTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutInOut_OutIn")]
		public void ShouldHandleAdherenceChanges(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));
			var siteId = events.OfType<PersonOutOfAdherenceEvent>().First().SiteId;
			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutDeleteInOut")]
		public void ShouldHandleDeletions(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));
			var siteId = events.OfType<PersonOutOfAdherenceEvent>().First().SiteId;
			Persister.Get(siteId).Count.Should().Be(0);
		}

		public static IEnumerable OutInOut_OutIn
		{
			get
			{
				var personId1 = Guid.NewGuid();
				var personId2 = Guid.NewGuid();
				var siteId = Guid.NewGuid();

				return permutationsOf(new List<IEvent>
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

					new PersonOutOfAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					}
				});
			}
		}

		public static IEnumerable OutDeleteInOut
		{
			get
			{
				var personId = Guid.NewGuid();
				var siteId = Guid.NewGuid();

				return permutationsOf(new List<IEvent>
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonDeletedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-18 12:04".Utc(),
						PersonPeriodsBefore = new[] {new PersonPeriodDetail { SiteId = siteId}}
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					},
				});
			}
		}

		private static IEnumerable permutationsOf(List<IEvent> events)
		{
			var permutations = events.Permutations();

			return from p in permutations
				   let name = (from pe in p select pe.GetType().Name).Aggregate((current, next) => current + ", " + next)
				   select new TestCaseData(p).SetName(name);
		}
	
	}
}