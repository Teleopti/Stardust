using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SpeedUpManualChanges_37029)]
	public class ResourceCalculationContextScopeTest
	{
		public ILifetimeScope LifetimeScope;

		[Test]
		public void ShouldMarkDayBeforeIfAffectedShiftStartedDayBeforeInUsersTimeZone()
		{
			const int repeat = 3;
			for (var i = 0; i < repeat; i++)
			{
				using (var scope = LifetimeScope.BeginLifetimeScope())
				{
					var schedulerStateHolder = scope.Resolve<Func<ISchedulerStateHolder>>();
					var scheduleDayChangeCallback = scope.Resolve<IScheduleDayChangeCallback>();

					ResourceCalculationContext.InContext.Should().Be.False();

					var date = new DateOnly(2015, 10, 12);
					var activity = new Activity("_");
					var scenario = new Scenario("_");
					var agent = new Person().WithId();
					var ass = new PersonAssignment(agent, scenario, date);
					ass.AddActivity(activity, new TimePeriod(0, 0, 1, 0));
					ass.SetShiftCategory(new ShiftCategory("_"));
					var stateHolder = schedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date.AddWeeks(-1), date.AddWeeks(1)), new[] { agent }, new[] { ass }, Enumerable.Empty<ISkillDay>());

					var schedule = stateHolder.Schedules[agent].ScheduledDay(date);
					schedule.DeleteMainShift();
					stateHolder.Schedules.Modify(schedule, scheduleDayChangeCallback);
				}
			}
		}
	}
}