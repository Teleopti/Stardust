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
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		[TestCaseSource(typeof(DetailsEventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));

			Persister.Model.Activities.Single().TimeInAdherence.Should().Be("2".Seconds());
			Persister.Model.Activities.Single().TimeOutOfAdherence.Should().Be("1".Seconds());
		}
	}

	public class DetailsEventsPermuationFactory
	{
		public static IEnumerable Permutations
		{
			get
			{
				var personId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonActivityStartEvent
					{
						PersonId = personId,
						StartTime = "2015-02-09 08:00:00".Utc(),
						Adherence = EventAdherence.In
					},
					new PersonStateChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:01".Utc(),
						Adherence = EventAdherence.Out
					},
					new PersonStateChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:02".Utc(),
						Adherence = EventAdherence.In
					},
					new PersonShiftEndEvent
					{
						PersonId = personId,
						ShiftStartTime = "2015-02-09 08:00:00".Utc(),
						ShiftEndTime = "2015-02-09 08:00:03".Utc()
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