using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[DomainTest]
	public class TeamDayOffModifierTest
	{
		public TeamDayOffModifier Target;
		public SchedulerStateHolder SchedulerStateHolder;
		public ITimeZoneGuard TimeZoneGuard;

		[Test]
		public void ShouldNotAddDayOffIfFullDayAbsence()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var scenario = new Scenario("unimportant");
			var absence = new Absence();
			var date = new DateOnly(2015, 10, 8);
			var selectedPeriod = new DateOnlyPeriod(2015, 10, 7, 2015, 10, 9);
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneInfo.Utc);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2015, 10, 7, 2015, 10, 9));
			var personAbsence = new PersonAbsence(agent, scenario, absenceLayer);
			scheduleDictionary.AddPersonAbsence(personAbsence);

			var rollBackService = new SchedulePartModifyAndRollbackService(
				SchedulerStateHolder.SchedulingResultState,
				new SchedulerStateScheduleDayChangedCallback(
						new ScheduleChangesAffectedDates(TimeZoneGuard),
					() => SchedulerStateHolder),
				new ScheduleTagSetter(new NullScheduleTag()));

			Target.AddDayOffForMember(rollBackService, agent, date, new DayOffTemplate(), false);

			var scheduleDayAfter = scheduleDictionary[agent].ScheduledDay(date);
			scheduleDayAfter.PersonAssignment(true).DayOff().Should().Be.Null();
		}

		[Test]
		public void ShouldAddDayOffIfMainShift()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var scenario = new Scenario("unimportant");
			var date = new DateOnly(2015, 10, 8);
			var selectedPeriod = new DateOnlyPeriod(2015, 10, 7, 2015, 10, 9);

			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneInfo.Utc);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var ass = new PersonAssignment(agent, scenario, date);
			var eightOClock = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0, DateTimeKind.Utc);
			var seventteenOClock = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0, DateTimeKind.Utc);
			ass.AddActivity(new Activity("hej"), new DateTimePeriod(eightOClock, seventteenOClock));
			ass.SetShiftCategory(new ShiftCategory("blajj"));
			scheduleDictionary.AddPersonAssignment(ass);

			var rollBackService = new SchedulePartModifyAndRollbackService(
				SchedulerStateHolder.SchedulingResultState,
				new SchedulerStateScheduleDayChangedCallback(
						new ScheduleChangesAffectedDates(TimeZoneGuard),
					() => SchedulerStateHolder),
				new ScheduleTagSetter(new NullScheduleTag()));

			Target.AddDayOffForMember(rollBackService, agent, date, new DayOffTemplate(), false);

			var scheduleDayAfter = scheduleDictionary[agent].ScheduledDay(date);
			scheduleDayAfter.PersonAssignment(true).DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotDeleteDayOffIfContractDayOff()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var scenario = new Scenario("unimportant");
			var date = new DateOnly(2015, 10, 8);
			var selectedPeriod = new DateOnlyPeriod(2015, 10, 7, 2015, 10, 9);
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneInfo.Utc);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 10, 7, 2015, 10, 9));
			var personAbsence = new PersonAbsence(agent, scenario, absenceLayer);
			scheduleDictionary.AddPersonAbsence(personAbsence);

			var ass = new PersonAssignment(agent, scenario, date);
			ass.SetDayOff(new DayOffTemplate());
			scheduleDictionary.AddPersonAssignment(ass);

			var rollBackService = new SchedulePartModifyAndRollbackService(
				SchedulerStateHolder.SchedulingResultState,
				new SchedulerStateScheduleDayChangedCallback(
						new ScheduleChangesAffectedDates(TimeZoneGuard),
					() => SchedulerStateHolder),
				new ScheduleTagSetter(new NullScheduleTag()));

			Target.RemoveDayOffForMember(rollBackService, agent, date);

			var scheduleDayAfter = scheduleDictionary[agent].ScheduledDay(date);
			scheduleDayAfter.PersonAssignment().DayOff().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDeleteDayOffIfNotContractDayOff()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var scenario = new Scenario("unimportant");
			var date = new DateOnly(2015, 10, 8);
			var selectedPeriod = new DateOnlyPeriod(2015, 10, 7, 2015, 10, 9);

			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneInfo.Utc);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var ass = new PersonAssignment(agent, scenario, date);
			ass.SetDayOff(new DayOffTemplate());
			scheduleDictionary.AddPersonAssignment(ass);

			var rollBackService = new SchedulePartModifyAndRollbackService(
				SchedulerStateHolder.SchedulingResultState,
				new SchedulerStateScheduleDayChangedCallback(
						new ScheduleChangesAffectedDates(TimeZoneGuard),
					() => SchedulerStateHolder),
				new ScheduleTagSetter(new NullScheduleTag()));

			Target.RemoveDayOffForMember(rollBackService, agent, date);

			var scheduleDayAfter = scheduleDictionary[agent].ScheduledDay(date);
			scheduleDayAfter.PersonAssignment().DayOff().Should().Be.Null();
		}
	}
}