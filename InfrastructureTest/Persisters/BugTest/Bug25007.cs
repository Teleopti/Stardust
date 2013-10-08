using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	public class Bug25007 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			return null;
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[0];
		}

		[Test]
		public void CanDoOverwriteWhenHavingTwoNewAssignments()
		{
			ScheduleDictionaryConflictCollector = new ScheduleDictionaryConflictCollector(ScheduleRepository, PersonAssignmentRepository, new LazyLoadingManagerWrapper(), new UtcTimeZone());
			MakeTarget();

			// me, in application
			AddPersonAssignmentInMemory(FirstDayDateOnly);
			
			// another user, in database
			AddPersonAssignmentAsAnotherUser(FirstDayDateOnly);

			//will give conflict
			var conflict = TryPersistScheduleScreen();

			((ScheduleRange)ScheduleDictionary[Person]).SolveConflictBecauseOfExternalInsert(conflict.ScheduleDictionaryConflicts.Single().DatabaseVersion, false);

			//should persist
			var result = TryPersistScheduleScreen();

			result.Saved.Should().Be.True();
		}	
	}
}