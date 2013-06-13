using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public class Bug11817 : ScheduleScreenPersisterIntegrationTest
	{
		private ShiftCategory _shiftCategory;
		private Activity _activity;
		private PersonAssignment _personAssignment;

		[SetUp]
		public void Bug11817Setup()
		{
			_shiftCategory = new ShiftCategory("sc");
			_activity = new Activity("sdfsdfsdf");
			PersistAndRemoveFromUnitOfWork(_shiftCategory);
			PersistAndRemoveFromUnitOfWork(_activity);
		}

		protected override IPersistableScheduleData MakeScheduleData()
		{
			return new Note(Person, FirstDayDateOnly, Scenario, "a test note");
		}

		[Test]
		public void PersonAccountConflictShouldWorkWithAddedScheduleData()
		{
			MakeTarget();

			ModifyPersonAbsenceAccountAsAnotherUser();

			ScheduleDictionary.TakeSnapshot();
			ModifyPersonAbsenceAccount();
			AddPersonAssignment();

			var result = TryPersistScheduleScreen();
			Assert.IsTrue(result.Saved);
		}

		private void AddPersonAssignment()
		{
			_personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			var mainShift = new MainShift(_shiftCategory);
			var layer = new MainShiftActivityLayer(_activity, FirstDayDateTimePeriod);
			mainShift.LayerCollection.Add(layer);
			_personAssignment.SetMainShift(mainShift);

			var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);
			scheduleDay.Add(_personAssignment);
            ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new Repository(unitOfWork);
				var personAssignment = new PersonAssignmentRepository(unitOfWork).Get(_personAssignment.Id.Value);
				repository.Remove(personAssignment);
				repository.Remove(_shiftCategory);
				repository.Remove(_activity);
				unitOfWork.PersistAll();
			}
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] {_shiftCategory, _activity};
		}

	}
}
