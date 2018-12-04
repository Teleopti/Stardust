using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreReadByDateTest
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
				BelongsToDate = "2018-10-31".Date()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-10-31".Date()));

			actual.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldLoadForPerson()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-10-31".Date()
			});
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = Guid.NewGuid(),
				BelongsToDate = "2018-11-01".Date()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-10-31".Date()));

			actual.Cast<PersonStateChangedEvent>().Single().PersonId.Should().Be(personId);
		}
		
		[Test]
		public void ShouldLoadForDate()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-10-31".Date()
			});
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-11-01".Date()
			});

			var actual = WithUnitOfWork.Get(() => Events.Load(personId, "2018-10-31".Date()));

			actual.Cast<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-31".Date());
		}
	}
}