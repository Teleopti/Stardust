using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CalculateFarAwayTimeZones_40646)]
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
			var period = new DateOnlyPeriod(date, date.AddWeeks(1));
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, new[] {ass}, Enumerable.Empty<ISkillDay>());
			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);

			schedule.DeleteMainShift();
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);

			stateHolder.DaysToRecalculate.Should().Contain(date.AddDays(-1));
		}

		[Test]
		public void ShouldMarkDayBeforeIfAffectingShiftStartsDayBeforeInUsersTimeZone()
		{
			FakeTimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var date = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(date, date.AddWeeks(1));
			var activity = new Activity("_");
			var scenario = new Scenario("_");
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());	
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), Enumerable.Empty<ISkillDay>());
			var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));

			schedule.AddMainShift(ass);
			stateHolder.Schedules.Modify(schedule, ScheduleDayChangeCallback);

			stateHolder.DaysToRecalculate.Should().Contain(date.AddDays(-1));
		}
	}
}