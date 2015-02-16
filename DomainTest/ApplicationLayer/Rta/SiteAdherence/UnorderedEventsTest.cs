using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.SiteAdherence
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
	}

	public class EventsPermuationFactory
	{
		public static IEnumerable Permutations
		{
			get
			{
				var personId = Guid.NewGuid();
				var siteId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = Guid.NewGuid(),
						SiteId = siteId
					}
				};
				var permutations = events.Permutations();
				return from p in permutations
					   let name = (from pe in p select pe.GetType().Name).Aggregate((current, next) => current + ", " + next)
					   select new TestCaseData(p).SetName(name);
			}
		}
	}
}