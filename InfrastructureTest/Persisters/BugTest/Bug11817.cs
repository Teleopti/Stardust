using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug11817 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			return null;
		}

		[Test]
		public void PersonAccountConflictShouldWorkWithAddedScheduleData()
		{
			MakeTarget();

			ModifyPersonAbsenceAccountAsAnotherUser();

			ScheduleDictionary.TakeSnapshot();
			ModifyPersonAbsenceAccountInMemory();
			AddPersonAssignmentInMemory(FirstDayDateOnly);

			var result = TryPersistScheduleScreen();
			Assert.IsTrue(result.Saved);
		}


		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}
