using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest : IRegisterInContainer
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherenceDetailsReadModelUpdater>().AsSelf();
		}

		[Test]
		[TestCaseSource(typeof(DetailsEventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));

			Persister.Model.Details.Single().TimeInAdherence.Should().Be("2".Seconds());
			Persister.Model.Details.Single().TimeOutOfAdherence.Should().Be("1".Seconds());
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
						InAdherence = true
					},
					new PersonStateChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:01".Utc(),
						InAdherence = false
					},
					new PersonStateChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:02".Utc(),
						InAdherence = true
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