using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug11817 : ScheduleScreenPersisterIntegrationTest
	{
		//private PersonAssignment _personAssignment;

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
			//AddPersonAssignment();

			var result = TryPersistScheduleScreen();
			Assert.IsTrue(result.Saved);
		}

		//private void AddPersonAssignment()
		//{
		//	_personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
		//	_personAssignment.SetMainShiftLayers(new[]{new MainShiftLayer(Activity, FirstDayDateTimePeriod)}, ShiftCategory);

		//	var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);
		//	scheduleDay.Add(_personAssignment);
		//	ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		//}

		//protected override void TeardownForRepositoryTest()
		//{
		//	using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
		//	{
		//		var repository = new Repository(unitOfWork);
		//		var personAssignment = new PersonAssignmentRepository(unitOfWork).Get(_personAssignment.Id.Value);
		//		repository.Remove(personAssignment);
		//		unitOfWork.PersistAll();
		//	}
		//}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}
