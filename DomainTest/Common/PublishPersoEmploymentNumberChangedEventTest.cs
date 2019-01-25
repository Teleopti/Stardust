using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PublishPersoEmploymentNumberChangedEventTest
	{
		public MutableNow Now;

		[Test]
		public void ShouldPublishWhenChangingEmploymentNumber()
		{
			var person = PersonFactory.CreatePerson();

			person.SetEmploymentNumber("123");

			((Person)person).PopAllEvents(null).OfType<PersonEmploymentNumberChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithId(personId);

			person.SetEmploymentNumber("123");

			var @event = ((Person)person).PopAllEvents(null).OfType<PersonEmploymentNumberChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.EmploymentNumber.Should().Be("123");
		}
	}
}