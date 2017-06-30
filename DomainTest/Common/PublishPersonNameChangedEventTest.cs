using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PublishPersonNameChangedEventTest
	{
		public MutableNow Now;

		[Test]
		public void ShouldPublishWhenChangingName()
		{
			var person = PersonFactory.CreatePerson();

			((Person)person).PopAllEvents().OfType<PersonNameChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithId(personId);
			((Person)person).PopAllEvents();

			person.SetName(new Name("bill","gates"));
		
	
			var @event = ((Person)person).PopAllEvents().OfType<PersonNameChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.FirstName.Should().Be("bill");
			@event.LastName.Should().Be("gates");
		}

		[Test]
		public void ShouldNotPublishIfNotChanged()
		{
			var person = PersonFactory.CreatePerson();
			((Person)person).PopAllEvents();
			person.SetName(new Name("bill", "gates"));

			((Person)person).PopAllEvents();
			person.SetName(new Name("bill", "gates"));
			var x = ((Person) person).PopAllEvents().OfType<PersonNameChangedEvent>();
			x.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishOnceForCumulativeChangesWithProperties()
		{
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithId(personId);
			((Person) person).PopAllEvents();
			
			person.SetName(new Name("bill", "rates"));
			person.SetName(new Name("bill", "gates"));
			

			var @event = ((Person) person).PopAllEvents().OfType<PersonNameChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.FirstName.Should().Be("bill");
			@event.LastName.Should().Be("gates");
		}
}

	
}