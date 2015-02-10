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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage
{
	[AdherenceTest]
	[TestFixture]
	public class UnorderedEventsTest : IRegisterInContainer
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherencePercentageReadModelUpdater>().AsSelf();
		}

		[Test]
		[TestCaseSource(typeof(PercentageEventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{

			events.ForEach(e => Target.Handle((dynamic)e));

			Persister.PersistedModel.TimeInAdherence.Should().Be("2".Seconds());
			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("1".Seconds());
		}
	}

	public class PercentageEventsPermuationFactory
	{
		public static IEnumerable Permutations
		{
			get
			{
				var personId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:00".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:01".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-09 08:00:02".Utc()
					},
					new PersonShiftEndEvent
					{
						PersonId = personId,
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