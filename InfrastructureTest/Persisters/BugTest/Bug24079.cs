using System.Collections.Generic;
using System.Linq;
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
			var myPersonAssignment = ScheduleData as IPersonAssignment;

			// another user, in database
			var otherUsersPersonAssignment = AddPersonAssignmentAsAnotherUser(FirstDayDateOnly);

			var result = TryPersistScheduleScreen();

			result.Saved.Should().Be.False();
			result.ScheduleDictionaryConflicts.Should().Have.Count.EqualTo(1);
			result.ScheduleDictionaryConflicts.Single().ClientVersion.OriginalItem.Should().Be.SameInstanceAs(myPersonAssignment);
			result.ScheduleDictionaryConflicts.Single().DatabaseVersion.Id.Should().Be(otherUsersPersonAssignment.Id);
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}