using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[DomainTest]
	public class OptionalColumnValueChangedEventTest
	{
		[Test]
		public void ShouldAddPersonToEvent()
		{
			var @event = new OptionalColumnValueChangedEvent();
			var personId = Guid.NewGuid();

			@event.PersonId = personId;
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWhenOptionalColumnValueIsSet()
		{
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var optionalColumn = new OptionalColumn("Car");
			person.SetOptionalColumnValue(new OptionalColumnValue("Saab"), optionalColumn);

			((Person)person).PopAllEvents(null).OfType<OptionalColumnValueChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenOptionalColumnValueIsRemoved()
		{
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var optionalColumn = new OptionalColumn("Car").WithId();
			person.SetOptionalColumnValue(new OptionalColumnValue("Saab"), optionalColumn);
			person.RemoveOptionalColumnValue(person.GetColumnValue(optionalColumn));

			((Person)person).PopAllEvents(null).OfType<OptionalColumnValueChangedEvent>().Count().Should().Be(1);
		}

		[Test]
		public void ShouldOnlyPublishOneEventForSeveralColumnValueChangesOnOnePerson()
		{
			var optionalColumn = new OptionalColumn("Car");
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());

			person.SetOptionalColumnValue(new OptionalColumnValue("Volvo"), optionalColumn);
			person.SetOptionalColumnValue(new OptionalColumnValue("Saab"), optionalColumn);

			((Person)person).PopAllEvents(null).OfType<OptionalColumnValueChangedEvent>().Count()
				.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyPublishOneEventForChangesOnTwoColumns()
		{
			var optionalColumn1 = new OptionalColumn("Car");
			var optionalColumn2 = new OptionalColumn("Home");
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());

			person.SetOptionalColumnValue(new OptionalColumnValue("Volvo"), optionalColumn1);
			person.SetOptionalColumnValue(new OptionalColumnValue("Bungalow"), optionalColumn2);

			((Person)person).PopAllEvents(null).OfType<OptionalColumnValueChangedEvent>().Count()
				.Should().Be(1);
		}
	}
}