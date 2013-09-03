﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug12887 : ScheduleScreenPersisterIntegrationTest
	{
		protected override IPersistableScheduleData SetupScheduleData()
		{
			var personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			personAssignment.AddMainLayer(Activity, FirstDayDateTimePeriod);
			personAssignment.SetShiftCategory(ShiftCategory);
			return personAssignment;
		}

		[Test]
		public void ShouldGiveConflictWithDeletedScheduleData()
		{
			ScheduleDictionaryConflictCollector = new ScheduleDictionaryConflictCollector(ScheduleRepository, PersonAssignmentRepository, new LazyLoadingManagerWrapper(), new UtcTimeZone());
			MakeTarget();

			DeleteScheduleDataAsAnotherUser();

			ScheduleDictionary.TakeSnapshot();
			modifyPersonAssignmanrInMemory();

			var result = TryPersistScheduleScreen();
			Assert.That(result.Saved, Is.False);
			Assert.That(result.ScheduleDictionaryConflicts.Count(), Is.EqualTo(1));
		}

		private void modifyPersonAssignmanrInMemory()
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