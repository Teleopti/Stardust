using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug12891 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			var personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			personAssignment.AddMainLayer(Activity, FirstDayDateTimePeriod);
			personAssignment.SetShiftCategory(ShiftCategory);
			return personAssignment;
		}

		[Test]
		public void PersonAccountConflictShouldWorkWithAddedLayer()
		{
			MakeTarget();

			//user 1
			ModifyPersonAbsenceAccountAsAnotherUser();

			//user 2
			ScheduleDictionary.TakeSnapshot();
			ModifyPersonAbsenceAccountInMemory();
			addMainShiftActivityLayer();
		
			var result = TryPersistScheduleScreen();
			Assert.IsTrue(result.Saved);
		}

		private void addMainShiftActivityLayer()
		{
			var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);

			var personAssignment = scheduleDay.PersonAssignment();
			personAssignment.AddMainLayer(personAssignment.MainLayers().First().Payload, FirstDayDateTimePeriod);
			ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}