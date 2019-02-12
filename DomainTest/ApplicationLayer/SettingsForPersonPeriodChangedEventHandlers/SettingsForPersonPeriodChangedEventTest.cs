using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[TestFixture]
	public class SettingsForPersonPeriodChangedEventTest
	{
		[Test]
		public void ShouldNotAddDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new SettingsForPersonPeriodChangedEvent();

			target.SetIdCollection(new [] { duplicateGuid, duplicateGuid });

			target.IdCollection.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnDuplicatesEvenIfSerializedWithDuplicates()
		{
			var duplicateGuid = Guid.NewGuid();
			var target = new SettingsForPersonPeriodChangedEvent {SerializedIds = $"{duplicateGuid},{duplicateGuid}"};

			target.IdCollection.Count().Should().Be.EqualTo(1);
		}
	}
}