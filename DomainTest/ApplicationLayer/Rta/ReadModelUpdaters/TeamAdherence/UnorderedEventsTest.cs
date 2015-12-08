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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		[TestCaseSource(typeof(EventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));
			var teamId = events.OfType<PersonOutOfAdherenceEvent>().First().TeamId;
			Persister.Get(teamId).Count.Should().Be(1);
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
				var teamId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonInAdherenceEvent
					{
						PersonId = personId1,
						TeamId = teamId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId1,
						TeamId = teamId,
						Timestamp = "2015-02-18 12:04".Utc()
					},

					new PersonInAdherenceEvent
					{
						PersonId = personId2,
						TeamId = teamId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId2,
						TeamId = teamId,
						Timestamp = "2015-02-18 12:08".Utc()
					},
					new PersonDeletedEvent
					{
						PersonId = personId2,
						Timestamp = "2015-02-18 12:09".Utc(),
						PersonPeriodsBefore = new[] {new PersonPeriodDetail {TeamId = teamId}}
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