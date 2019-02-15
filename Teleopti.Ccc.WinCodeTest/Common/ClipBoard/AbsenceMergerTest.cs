using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
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

		[Test]
		public void ShouldMergeFullDayAbsenceInstersectingToOneAbsence()
		{
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var absence = new Absence().WithId();
			var expectedPeriod = dateBefore.ToDateTimePeriod(new TimePeriod(17, 27 + 24), TimeZoneInfo.Utc);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsenceBefore = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, dateBefore.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(new TimePeriod(2, 27), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAssignment };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDayBefore = stateHolder.Schedules[agent].ScheduledDay(dateBefore);
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			scheduleDayBefore.Add(personAbsenceBefore);
			scheduleDay.Add(personAbsence);
			var list = new List<IScheduleDay> {scheduleDayBefore, scheduleDay};
			var absenceMerger = new AbsenceMerger(list);

			absenceMerger.MergeWithDayBefore();

			list.Single().PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(expectedPeriod);
		}

		[Test]
		public void ShouldMergeAbsencesOnDayStart()
		{
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var absence = new Absence().WithId();
			var expectedPeriod = date.ToDateTimePeriod(new TimePeriod(0, 4), TimeZoneInfo.Utc);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var personAbsence1 = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(new TimePeriod(0, 3), TimeZoneInfo.Utc)));
			var personAbsence2 = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(new TimePeriod(0, 4), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignment };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			scheduleDay.Add(personAbsence1);
			scheduleDay.Add(personAbsence2);
			var list = new List<IScheduleDay> { scheduleDay };
			var absenceMerger = new AbsenceMerger(list);

			absenceMerger.MergeOnDayStart();

			list.Single().PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(expectedPeriod);
		}

		[Test]
		public void ShouldMergeAbsenceOnMultipleAgents()
		{
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var absence = new Absence().WithId();
			var expectedPeriod = dateBefore.ToDateTimePeriod(new TimePeriod(17, 27 + 24), TimeZoneInfo.Utc);
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment1 = new PersonAssignment(agent1, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignment2 = new PersonAssignment(agent2, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentBefore1 = new PersonAssignment(agent1, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentBefore2 = new PersonAssignment(agent2, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsenceBefore1 = new PersonAbsence(agent1, scenario, new AbsenceLayer(absence, dateBefore.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var personAbsenceBefore2 = new PersonAbsence(agent2, scenario, new AbsenceLayer(absence, dateBefore.ToDateTimePeriod(new TimePeriod(17, 27), TimeZoneInfo.Utc)));
			var personAbsence1 = new PersonAbsence(agent1, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(new TimePeriod(2, 27), TimeZoneInfo.Utc)));
			var personAbsence2 = new PersonAbsence(agent2, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(new TimePeriod(2, 27), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore1, personAssignmentBefore2, personAssignment1 , personAssignment2};
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent1, agent2 }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDayBefore1 = stateHolder.Schedules[agent1].ScheduledDay(dateBefore);
			var scheduleDayBefore2 = stateHolder.Schedules[agent2].ScheduledDay(dateBefore);
			var scheduleDay1 = stateHolder.Schedules[agent1].ScheduledDay(date);
			var scheduleDay2 = stateHolder.Schedules[agent2].ScheduledDay(date);
			scheduleDayBefore1.Add(personAbsenceBefore1);
			scheduleDayBefore2.Add(personAbsenceBefore2);
			scheduleDay1.Add(personAbsence1);
			scheduleDay2.Add(personAbsence2);
			var list = new List<IScheduleDay> { scheduleDayBefore1, scheduleDayBefore2, scheduleDay1, scheduleDay2 };
			var absenceMerger = new AbsenceMerger(list);

			absenceMerger.MergeWithDayBefore();

			list.Count.Should().Be.EqualTo(2);
			list.First().PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(expectedPeriod);
			list.Second().PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(expectedPeriod);
		}
	}
}
