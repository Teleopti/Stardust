﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[Toggle(Toggles.ResourcePlanner_CalculateFarAwayTimeZones_40646)]
	[DomainTest]
	public class ResourceCalculateCorrectDaysTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IScheduleDayChangeCallback ScheduleDayChangeCallback;
		public FakeTimeZoneGuard FakeTimeZoneGuard;

		[Test]
		public void ShouldMarkDayBeforeIfAffectedShiftStartedDayBeforeInUsersTimeZone()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12); 
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));
			ass.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, new[] {ass}, Enumerable.Empty<ISkillDay>());

			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			schedule.DeleteMainShift();
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);

			stateHolder.DaysToRecalculate.Should().Have.SameValuesAs(date.AddDays(-1));
		}

		[Test]
		public void ShouldMarkDayBeforeIfAffectingShiftStartsDayBeforeInUsersTimeZone()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());

			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));
			ass.SetShiftCategory(new ShiftCategory("_"));
			schedule.AddMainShift(ass);
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);

			//Claes! ska detta vara enbart -1 eller -1 och 0?
			stateHolder.DaysToRecalculate.Should().Contain(date.AddDays(-1));
		}

		[Test]
		public void ShouldMarkDayAndDayAfterWhenAffectingShiftStartsDayAfterInUsersTimeZone()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			var assPrevious = new PersonAssignment(agent, scenario, date);
			assPrevious.AddActivity(activity, new TimePeriod(10,0, 11, 0));
			assPrevious.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, new[] { assPrevious }, Enumerable.Empty<ISkillDay>());

			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			schedule.DeleteMainShift();
			var assCurrent = new PersonAssignment(agent, scenario, date);
			assCurrent.AddActivity(activity, new TimePeriod(23, 45, 24, 45));
			assCurrent.SetShiftCategory(new ShiftCategory("_"));
			schedule.AddMainShift(assCurrent);
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);

			stateHolder.DaysToRecalculate.Should().Have.SameValuesAs(date, date.AddDays(1));
		}

		[Test]
		public void ShouldMarkDayWhenUndo()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assPrevious = new PersonAssignment(agent, scenario, date);
			assPrevious.AddActivity(activity, new TimePeriod(10, 0, 11, 0));
			assPrevious.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, new[] { assPrevious }, Enumerable.Empty<ISkillDay>());
			var undoRedoContainer = new UndoRedoContainer(ScheduleDayChangeCallback, 10);
			stateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);

			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			schedule.DeleteMainShift();
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);
			stateHolder.ClearDaysToRecalculate();
			undoRedoContainer.Undo();

			stateHolder.DaysToRecalculate.Should().Have.SameValuesAs(date);
		}

		[Test]
		public void ShouldMarkDayWhenRedo()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assPrevious = new PersonAssignment(agent, scenario, date);
			assPrevious.AddActivity(activity, new TimePeriod(10, 0, 11, 0));
			assPrevious.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, new[] { assPrevious }, Enumerable.Empty<ISkillDay>());
			var undoRedoContainer = new UndoRedoContainer(ScheduleDayChangeCallback, 10);
			stateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);

			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			schedule.DeleteMainShift();
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);
			undoRedoContainer.Undo();
			stateHolder.ClearDaysToRecalculate();
			undoRedoContainer.Redo();

			stateHolder.DaysToRecalculate.Should().Have.SameValuesAs(date);
		}
	}
}