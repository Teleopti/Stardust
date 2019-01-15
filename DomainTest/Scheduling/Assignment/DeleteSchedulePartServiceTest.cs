using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
    public class DeleteSchedulePartServiceTest
    {
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public ITimeZoneGuard TimeZoneGuard;

        [Test]
        public void ShouldNotDeleteDayOffUnderFullDayAbsence()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).WithDayOff().WithId();
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(0, 24), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignment, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = true };

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, deleteOption, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).HasDayOff().Should().Be.True();
		}

		[Test]
		public void ShouldDeleteMainShift([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault, [Values(true, false)] bool useMainShiftSpecial)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));	
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = useDefault, MainShift = !useDefault && !useMainShiftSpecial, MainShiftSpecial = !useDefault && useMainShiftSpecial};
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date)}, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date)}, rollBackService);
			
			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteFullDayAbsence([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(8, 17), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignment, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = useDefault, Absence = !useDefault };
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date)}, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, rollBackService);

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeletePartDayAbsence([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(8, 10), TimeZoneInfo.Utc)));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new [] { personAbsence }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = useDefault, Absence = !useDefault };
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, rollBackService);

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeletePersonalShift([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId());
			personAssignment.AddPersonalActivity(activity, date.ToDateTimePeriod(new TimePeriod(8, 17), TimeZoneInfo.Utc));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = useDefault, PersonalShift = !useDefault };
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, rollBackService);

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().PersonalActivities().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteDayOff([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).WithDayOff().WithId();
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption { Default = useDefault, DayOff = !useDefault };
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, rollBackService);

			stateHolder.Schedules[agent].ScheduledDay(date).HasDayOff().Should().Be.False();
		}

		[Test]
		public void ShouldDeleteOvertime([Values(true, false)] bool useProgress, [Values(true, false)] bool useDefault)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId());
			personAssignment.AddOvertimeActivity(activity, date.ToDateTimePeriod(new TimePeriod(8, 17), TimeZoneInfo.Utc), MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption{Default = useDefault, Overtime = !useDefault};
			if (useProgress)
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, deleteOption, rollBackService, new NoSchedulingProgress());
			else
				target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, rollBackService);

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().OvertimeActivities().Should().Be.Empty();
		}

		[Test]
        public void VerifyDeletePreference()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var absenceRestriction = new PreferenceRestriction { Absence = new Absence().WithId() };
			var preferenceDay = new PreferenceDay(agent, date, absenceRestriction).WithId();
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { preferenceDay }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Preference = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonRestrictionCollection().Should().Be.Empty();
		}

		[Test]
        public void VerifyStudentAvailabilityRestriction()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null), EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(20)) };
			var studentAvailabilityDay = new StudentAvailabilityDay(agent, date, new IStudentAvailabilityRestriction[] { studentAvailabilityRestriction });
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { studentAvailabilityDay }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { StudentAvailability = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonRestrictionCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteWithSpecifiedRules()
		{
			var rules = NewBusinessRuleCollection.Minimum();
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment1 = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAssignment2 = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment1, personAssignment2 }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			var result = target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress(), rules);

			result.Count.Should().Be.EqualTo(2);
		}

        [Test]
        public void VerifyDeleteIsReturningListOfNewScheduleParts()
        {
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignment1 = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAssignment2 = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAssignment1, personAssignment2 }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			var result = target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date), stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotDeleteAbsenceFromDayBefore()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(20, 27), TimeZoneInfo.Utc)));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] {agent}, new []{personAbsence}, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
					new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(dateBefore.AddDays(1)) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldDeletePartDayAbsenceSpanningOverMidnight()
		{
			var target = new DeleteSchedulePartService();	
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(20, 27), TimeZoneInfo.Utc)));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, new[] { personAbsence }, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotChangeAbsenceFromDayBeforeEndingTodayWhenDeletingFullDayAbsence()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 17));
			var personAbsenceDayBeforePeriod = dateBefore.ToDateTimePeriod(new TimePeriod(8, 27), TimeZoneInfo.Utc);
			var personAbsenceDayBefore = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), personAbsenceDayBeforePeriod));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(8, 27), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAssignment, personAbsenceDayBefore, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(dateBefore).PersonAbsenceCollection().First().Layer.Period.Should().Be.EqualTo(personAbsenceDayBeforePeriod);
		}

		[Test]
		public void ShouldSplitAndDeleteFromFullDayAbsenceSpanningMultipleDays()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(0, 48), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(dateBefore).PersonAbsenceCollection().First().Layer.Period.Should().Be.EqualTo(dateBefore.ToDateTimePeriod(new TimePeriod(0, 24), TimeZoneInfo.Utc));
			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldSplitAndDeleteFromFullDayAbsenceSpanningMultipleDaysWithNightShiftBeneath()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(0, 51), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> {personAssignmentBefore, personAssignment, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(dateBefore).IsFullDayAbsence().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(date).IsFullDayAbsence().Should().Be.False();
		}

		[Test]
		public void ShouldSplitAndDeleteFromFullDayAbsenceSpanningMultipleDaysWithNightShiftBeneathOnFirstDay()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(0, 51), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(dateBefore).IsFullDayAbsence().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(date).IsFullDayAbsence().Should().Be.False();
		}

		[Test]
		public void ShouldSplitAndDeleteFromFullDayAbsenceSpanningMultipleDaysWithDayOff()
		{
			var target = new DeleteSchedulePartService();
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).WithDayOff(new DayOffTemplate());
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(0, 48), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignment, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(dateBefore).IsFullDayAbsence().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveAbsenceOnWholeDayHavingMultipleDaySpanWithShiftBeneath()
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var activity = new Activity("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(8, 16));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), date.ToDateTimePeriod(new TimePeriod(0, 48), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignment, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));

			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date) }, new DeleteOption { Default = true }, rollBackService, new NoSchedulingProgress());

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Should().Be.Empty();
			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)).IsFullDayAbsence().Should().Be.True();
		}

		[TestCase(true)]
		[TestCase(false)]
		[Ignore("_")]
		public void ShouldNotCreateDuplicateLayersOnRollback(bool defaultDelete)
		{
			var target = new DeleteSchedulePartService();
			var date = new DateOnly(2018, 10, 1);
			var scenario = new Scenario().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var period = date.ToDateTimePeriod(new TimePeriod(0, 48),TimeZoneInfo.Utc);
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), period));
			var data = new List<IPersistableScheduleData> { personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var rollBackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new SchedulerStateScheduleDayChangedCallback(
				new ScheduleChangesAffectedDates(TimeZoneGuard), () => stateHolder), new ScheduleTagSetter(new NullScheduleTag()));
			var deleteOption = new DeleteOption {Absence = !defaultDelete, Default = defaultDelete};
			target.Delete(new[] { stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)) }, deleteOption, rollBackService, new NoSchedulingProgress(), new FakeNewBusinessRuleCollection());

			rollBackService.Rollback();

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(period);
			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)).PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(period);
		}
	}
}
