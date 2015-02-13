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
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.TeamAdherence
{
	[AdherenceTest]
	[TestFixture, Ignore]
	public class UnorderedEventsTest : IRegisterInContainer
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TeamOutOfAdherenceReadModelUpdater>().AsSelf();
			
		}

		[Test]
		[TestCaseSource(typeof(EventsPermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e => Target.Handle((dynamic)e));
			var teamId = events.OfType<PersonOutOfAdherenceEvent>().First().TeamId;
			Persister.Get(teamId).Count.Should().Be(2);
		}

	}

	public class EventsPermuationFactory
	{
		public static IEnumerable Permutations
		{
			get
			{
				var personId = Guid.NewGuid();
				var teamId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						TeamId = teamId
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						TeamId = teamId
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						TeamId = teamId
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = Guid.NewGuid(),
						TeamId = teamId
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