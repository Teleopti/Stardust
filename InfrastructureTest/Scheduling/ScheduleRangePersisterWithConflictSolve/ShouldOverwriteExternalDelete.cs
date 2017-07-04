﻿using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling.ScheduleRangePersisterWithConflictSolve
{
	public class ShouldOverwriteExternalDelete : ScheduleRangePersisterWithConflictSolveBase
	{
		private DateTimePeriod _expectedActivityPeriod;
		private PersonAssignment _personAssignment;

		protected override void CreateBaseSchedules()
		{
			_personAssignment = new PersonAssignment(Agent, Scenario, StartDate);
			PersonAssignmentRepository.Add(_personAssignment);
		}

		protected override void ModifyFirst(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);
			scheduleDay.Remove(_personAssignment);
			DoModify(scheduleDay);
		}

		protected override void ModifySecond(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);

			_expectedActivityPeriod =
				new DateTimePeriod(
					new DateTime(StartDate.Date.Year, StartDate.Date.Month, StartDate.Date.Day, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(StartDate.Date.Year, StartDate.Date.Month, StartDate.Date.Day, 10, 0, 0, DateTimeKind.Utc));
			scheduleDay.CreateAndAddActivity(Activity, _expectedActivityPeriod);
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
			activity.Period.Should().Be.EqualTo(_expectedActivityPeriod);
		}
	}
}