using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug24079 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			return null;
		}

		[Test]
		public void ShouldGiveConflictWithPersonAssignmentConstraintViolation()
		{
			MakeTarget();

			// me, in application
			AddPersonAssignmentInMemory(FirstDayDateOnly);

			// another user, in database
			AddPersonAssignmentAsAnotherUser(FirstDayDateOnly);

			var result = TryPersistScheduleScreen();

			result.Saved.Should().Be.False();
			result.ScheduleDictionaryConflicts.Should().Have.Count.EqualTo(1);
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}