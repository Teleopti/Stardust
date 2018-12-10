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
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.BusinessRules
{
	[DomainTest]
	public class MinWeeklyRestRuleTest
	{
		public Func<ISchedulerStateHolder> StateHolder;
		
		[Test]
		public void ShouldBreakWeeklyRestWhenLateShiftBeforeTwoConsecutiveDaysOf()
		{
			var shiftCategory = new ShiftCategory("_");
			var dateFirstDayOfWeek = new DateOnly(2017, 6, 5);
			var unimportant = TimeSpan.FromHours(5);
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(unimportant, unimportant, unimportant, TimeSpan.FromHours(48))
			};
			var agent = new Person().WithPersonPeriod(contract);
			var scenario = new Scenario("_");
			var activity = new Activity {InWorkTime = true};
			var asses = new[]
			{
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(-1)).WithLayer(activity, new TimePeriod(17, 25)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(0)).WithLayer(activity, new TimePeriod(8,16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(1)).WithLayer(activity, new TimePeriod(8,16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(2)).WithLayer(activity, new TimePeriod(8,16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(3)).WithLayer(activity, new TimePeriod(23, 24 + 7)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(4)).WithDayOff(),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(5)).WithDayOff(),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(6)).WithLayer(activity, new TimePeriod(6,14)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, dateFirstDayOfWeek.AddDays(7)).WithLayer(activity, new TimePeriod(0,8)).ShiftCategory(shiftCategory)
			};
			var stateHolder = StateHolder.Fill(scenario, new DateOnlyPeriod(2017, 6, 1, 2017, 7, 1), new[] {agent}, asses, Enumerable.Empty<ISkillDay>());

			var scheduleToChange = stateHolder.Schedules[agent].ScheduledDay(dateFirstDayOfWeek.AddDays(3));
			scheduleToChange.PersonAssignment(true).AddActivity(activity, new TimePeriod(23, 24 + 7));
			stateHolder.Schedules.Modify(scheduleToChange, NewBusinessRuleCollection.All(stateHolder.SchedulingResultState), true);

			stateHolder.Schedules[agent].ScheduledDay(dateFirstDayOfWeek.AddDays(3)).BusinessRuleResponseCollection
				.Where(x => x.TypeOfRule == typeof(MinWeeklyRestRule))
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotGenerateBrokenWeeklyRestDayOffAndAbsenceInvolved()
		{
			var firstDayOfWeek = new DateOnly(2018, 3, 12);
			var scenario = new Scenario();
			var activity = new Activity { InWorkTime = true };
			var contract = new Contract("_") {WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(11), TimeSpan.FromHours(31))};
			var agent = new Person().WithPersonPeriod(contract);
			var data = new List<IPersistableScheduleData>
			{
				new PersonAssignment(agent, scenario, firstDayOfWeek).ShiftCategory(new ShiftCategory()),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(1)).WithDayOff(),
				new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence(), firstDayOfWeek.AddDays(2).ToDateTimePeriod(TimeZoneInfo.Utc)))
			};
			var stateHolder = StateHolder.Fill(scenario, new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(7)), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(firstDayOfWeek);
			scheduleDay.PersonAssignment().AddActivity(activity, new TimePeriod(8, 17));

			stateHolder.Schedules.Modify(scheduleDay, NewBusinessRuleCollection.All(stateHolder.SchedulingResultState)).Should()
				.Be.Empty();
		}

		[Test]
		public void ShouldNotGenerateBrokenWeeklyRestWhenAbsenceInNextWeekShouldEnsureRest()
		{
			var firstDayOfWeek = new DateOnly(2018, 3, 12);
			var scenario = new Scenario();
			var activity = new Activity { InWorkTime = true };
			var absence = new Absence();
			var shiftCategory = new ShiftCategory("_");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(48), TimeSpan.FromHours(11), TimeSpan.FromHours(48)) };
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract);
			var dayOffTemplate = new DayOffTemplate(new Description("_")) { Anchor = TimeSpan.FromHours(12) };
			dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromDays(1), TimeSpan.Zero);
			var data = new List<IPersistableScheduleData>
			{
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(-1)).WithLayer(activity, new TimePeriod(17, 24)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(0)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(2)).WithDayOff(dayOffTemplate),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(3)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(4)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(5)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(6)).WithDayOff(dayOffTemplate),
				new PersonAssignment(agent, scenario, firstDayOfWeek.AddDays(7)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory),
				new PersonAbsence(agent, scenario, new AbsenceLayer(absence, firstDayOfWeek.AddDays(7).ToDateTimePeriod(TimeZoneInfo.Utc)))
			};
			var stateHolder = StateHolder.Fill(scenario, new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(7)), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(firstDayOfWeek);
			scheduleDay.PersonAssignment().AddActivity(activity, new TimePeriod(8, 17));

			stateHolder.Schedules.Modify(scheduleDay, NewBusinessRuleCollection.All(stateHolder.SchedulingResultState)).Should()
				.Be.Empty();
		}
 	}
}