using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	public class PersistWriteProtectionTest : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			return new Note(Person, FirstDayDateOnly, Scenario, "sdf");
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate() { return new IAggregateRoot[] {}; }

		[Test]
		public void CanPersistAndReadWriteProtection()
		{
			Person.PersonWriteProtection.PersonWriteProtectedDate = FirstDayDateOnly;
			MakeTarget();
			PersonWriteProtectionInfoCollection.Add(Person.PersonWriteProtection);
			TryPersistScheduleScreen();

			Session.Evict(Person.PersonWriteProtection);
			Assert.AreEqual(FirstDayDateOnly, Session.Get<PersonWriteProtectionInfo>(Person.PersonWriteProtection.Id.Value).PersonWriteProtectedDate);
		}
	}
}