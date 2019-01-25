using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingHourlyDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		
		[Test]
		public void ShouldScheduleFixedStaffWhenUsingHourlyAvailability()
		{
			var date = new DateOnly(2017, 5, 15);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario {DefaultScenario = true};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 10);
			var studentAvailDays = new List<IStudentAvailabilityDay>();
			for (var i = 0; i < 4; i++)
			{
				studentAvailDays.Add(new StudentAvailabilityDay(agent, date.AddDays(i), new IStudentAvailabilityRestriction[]
					{
						new StudentAvailabilityRestriction
						{
							StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), null),
							EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(21, 0, 0))
						}
					}));
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), agent, studentAvailDays, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions{UseStudentAvailability = true}, new NoSchedulingProgress(), new[]{agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));
			
			for (var i = 0; i < 7; i++)
			{
				if (i < 4)
					stateHolder.Schedules[agent].ScheduledDay(date.AddDays(i)).PersonAssignment(true).ShiftLayers
						.Should().Not.Be.Empty();
				else
					stateHolder.Schedules[agent].ScheduledDay(date.AddDays(i)).PersonAssignment(true).AssignedWithDayOff(stateHolder.CommonStateHolder.DefaultDayOffTemplate)
						.Should().Be.True();
			}		
		}

		public SchedulingHourlyDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}