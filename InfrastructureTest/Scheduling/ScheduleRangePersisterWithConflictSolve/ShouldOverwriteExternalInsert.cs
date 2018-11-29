﻿using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Scheduling.ScheduleRangePersisterWithConflictSolve
{
	public class ShouldOverwriteExternalInsert : ScheduleRangePersisterWithConflictSolveBase
	{
		protected override void CreateBaseSchedules()
		{
		}

		protected override void ModifyFirst(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);
			var personAssignment = new PersonAssignment(Agent, Scenario, StartDate);
			personAssignment.SetDayOff(DayOffTemplate);
			scheduleDay.AddMainShift(personAssignment);
			DoModify(scheduleDay);
		}

		protected override void ModifySecond(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);
			var personAssignment = new PersonAssignment(Agent, Scenario, StartDate);
			personAssignment.AddActivity(Activity, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(10)));
			scheduleDay.AddMainShift(personAssignment);
			DoModify(scheduleDay);
		}

		protected override void VerifyResult(IScheduleDictionary result)
		{
			var scheduleDay = result[Agent].ScheduledDay(StartDate);
			var assignment = scheduleDay.PersistableScheduleDataCollection().FirstOrDefault() as IPersonAssignment;
			assignment.Should().Not.Be.Null();

			var activity = assignment.MainActivities().FirstOrDefault();
			activity.Should().Not.Be.Null();
			activity.Payload.Should().Be.EqualTo(Activity);
		}
	}
}