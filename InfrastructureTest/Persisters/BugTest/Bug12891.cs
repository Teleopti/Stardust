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
		private IShiftCategory _shiftCategory;
		private IActivity _activity;

		protected override IPersistableScheduleData MakeScheduleData()
		{
			_shiftCategory = new ShiftCategory("sc");
			_activity = new Activity("sdfsdfsdf");
			PersistAndRemoveFromUnitOfWork(_shiftCategory);
			PersistAndRemoveFromUnitOfWork(_activity);

			var personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			personAssignment.SetMainShiftLayers(new[]{new MainShiftLayer(_activity, FirstDayDateTimePeriod)}, _shiftCategory);
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

			var personAssignment = scheduleDay.PersonAssignmentCollection()[0];
			var orgLayers = new List<IMainShiftLayer>(personAssignment.MainShiftActivityLayers);
			orgLayers.Add(new MainShiftLayer(orgLayers.First().Payload, FirstDayDateTimePeriod));
			personAssignment.SetMainShiftLayers(orgLayers, personAssignment.ShiftCategory);
			ScheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new Repository(unitOfWork);
				rep.Remove(_shiftCategory);
				rep.Remove(_activity);
				unitOfWork.PersistAll();
			}
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { _shiftCategory, _activity };
		}

	}
}