using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug12891 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData MakeScheduleData()
		{
			var personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			personAssignment.SetMainShiftLayers(new[]{new MainShiftLayer(Activity, FirstDayDateTimePeriod)}, ShiftCategory);
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
			ModifyPersonAbsenceAccount();
			addMainShiftActivityLayer();
		
			var result = TryPersistScheduleScreen();
			Assert.IsTrue(result.Saved);
		}

		private void addMainShiftActivityLayer()
		{
			var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);

			var personAssignment = scheduleDay.PersonAssignment();
			var orgLayers = new List<IMainShiftLayer>(personAssignment.MainLayers());
			orgLayers.Add(new MainShiftLayer(orgLayers.First().Payload, FirstDayDateTimePeriod));
			personAssignment.SetMainShiftLayers(orgLayers, personAssignment.ShiftCategory);
			ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}