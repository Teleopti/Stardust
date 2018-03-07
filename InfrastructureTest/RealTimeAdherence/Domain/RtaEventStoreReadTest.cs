using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[Toggle(Toggles.RTA_RemoveApprovedOOA_47721)]
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
	}
}