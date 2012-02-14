﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug12887 : ScheduleScreenPersisterIntegrationTest
	{
		private IShiftCategory _shiftCategory;
		private IActivity _activity;

		protected override IPersistableScheduleData MakeScheduleData()
		{
			_shiftCategory = new ShiftCategory("sc");
			_activity = new Activity("sdfsdfsdf");
			PersistAndRemoveFromUnitOfWork(_shiftCategory);
			PersistAndRemoveFromUnitOfWork(_activity);

			var personAssignment = new PersonAssignment(Person, Scenario);
			var mainShift = new MainShift(_shiftCategory);
			var layer = new MainShiftActivityLayer(_activity, FirstDayDateTimePeriod);
			mainShift.LayerCollection.Add(layer);
			personAssignment.SetMainShift(mainShift);
			return personAssignment;
		}

		[Test]
		public void PersonAccountConflictShouldWorkWithDeletedScheduleData()
		{
			ScheduleDictionaryConflictCollector = new ScheduleDictionaryConflictCollector(ScheduleRepository, new LazyLoadingManagerWrapper());
			MakeTarget();

			DeleteScheduleDataAsAnotherUser();

			ScheduleDictionary.TakeSnapshot();
			ModifyScheduleData();

			var result = TryPersistScheduleScreen();
			Assert.That(result.Saved, Is.False);
			Assert.That(result.ScheduleDictionaryConflicts.Count(), Is.EqualTo(1));
		}

		private void ModifyScheduleData()
		{
			var scheduleDay = ScheduleDictionary[Person].ScheduledDay(FirstDayDateOnly);

			var personAssignment = scheduleDay.PersonAssignmentCollection()[0];
			var activity = personAssignment.MainShift.LayerCollection[0].Payload;
			var layer = new MainShiftActivityLayer(activity, FirstDayDateTimePeriod);
			personAssignment.MainShift.LayerCollection.Add(layer);

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