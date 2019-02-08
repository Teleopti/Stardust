using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[TestFixture]
	public class OptionalColumnCollectionChangedEventTest
	{
		[Test]
		public void ShouldNotAddDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new OptionalColumnCollectionChangedEvent();

			target.SetOptionalColumnIdCollection(new[] { duplicateGuid, duplicateGuid });

			target.OptionalColumnIdCollection.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnDuplicatesEvenIfSerializedWithDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new OptionalColumnCollectionChangedEvent { SerializedOptionalColumn = $"{duplicateGuid},{duplicateGuid}" };

			target.OptionalColumnIdCollection.Count().Should().Be.EqualTo(1);
		}
	}

}