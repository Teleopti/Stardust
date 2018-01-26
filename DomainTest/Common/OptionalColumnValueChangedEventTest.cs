using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
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
			var personId1 = Guid.NewGuid();

			@event.PersonId = personId1;
			@event.PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldPublishWhenOptionalColumnValueIsSet()
		{
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var optionalColumn = new OptionalColumn("Car");
			person.SetOptionalColumnValue(new OptionalColumnValue("Saab"), optionalColumn);

			optionalColumn.PopAllEvents().OfType<OptionalColumnValueChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenOptionalColumnValueIsRemoved()
		{
			var person = PersonFactory.CreatePersonWithId(Guid.NewGuid());
			var optionalColumn = new OptionalColumn("Car");
			person.SetOptionalColumnValue(new OptionalColumnValue("Saab"), optionalColumn);
			person.RemoveOptionalColumnValue(person.GetColumnValue(optionalColumn));

			optionalColumn.PopAllEvents().OfType<OptionalColumnValueChangedEvent>().Count().Should().Be(1);
		}
	}
}