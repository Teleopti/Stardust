using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers
{
	[TestFixture]
	public class PersonCollectionChangedEventTest
	{
		[Test]
		public void ShouldNotAddDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new PersonCollectionChangedEvent();

			target.SetPersonIdCollection(new[] { duplicateGuid, duplicateGuid });

			target.PersonIdCollection.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnDuplicatesEvenIfSerializedWithDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new PersonCollectionChangedEvent { SerializedPeople = $"{duplicateGuid},{duplicateGuid}" };

			target.PersonIdCollection.Count.Should().Be.EqualTo(1);
		}
	}
}