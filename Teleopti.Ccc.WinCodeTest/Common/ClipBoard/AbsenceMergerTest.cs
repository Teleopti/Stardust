using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
	[DomainTest]
	public class AbsenceMergerTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldMergeFullDayAbsenceFromDayBeforeIntersecting()
		{
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsenceBefore = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAssignment };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());

			var scheduleDayBefore = stateHolder.Schedules[agent].ScheduledDay(dateBefore);
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);

			scheduleDayBefore.Add(personAbsenceBefore);
			scheduleDay.Add(personAbsence);

			var absenceMerger = new AbsenceMerger(new[] { scheduleDayBefore, scheduleDay });
			absenceMerger.MergeWithDayBefore();

			scheduleDay.PersistableScheduleDataCollection().Should().Contain(personAbsenceBefore);
		}

		[Test]
		public void ShouldMergeFullDayAbsenceFromDayBeforeIntersectingAndNoAbsenceToday()
		{
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsenceBefore = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAssignment };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDayBefore = stateHolder.Schedules[agent].ScheduledDay(dateBefore);
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			scheduleDayBefore.Add(personAbsenceBefore);

			var absenceMerger = new AbsenceMerger(new[] { scheduleDayBefore, scheduleDay });
			absenceMerger.MergeWithDayBefore();

			scheduleDay.PersistableScheduleDataCollection().Should().Contain(personAbsenceBefore);
		}
	}
}
