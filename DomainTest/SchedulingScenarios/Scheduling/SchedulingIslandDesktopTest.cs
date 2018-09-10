using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingIslandDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public ReduceIslandsLimits ReduceIslandsLimits;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		
		[Test, Ignore("#76176")]
		public void ShouldHandleDifferentOpenHoursWhenReducingSkills()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			var scenario = new Scenario();
			var activity = new Activity{RequiresSkill = true}.WithId();
			var skillA = new Skill().For(activity).IsOpenBetween(0,12).WithId();
			var skillB = new Skill().For(activity).IsOpenBetween(12,24).WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var date = new DateOnly(2015, 10, 12);
			var agentToSchedule = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date);
			var otherAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA);
			var skillDays = new[]
			{
				skillA.CreateSkillDayWithDemand(scenario, date, 100),
				skillB.CreateSkillDayWithDemand(scenario, date, 100)
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, date, new[] { agentToSchedule, otherAgent }, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agentToSchedule}, date.ToDateOnlyPeriod());
			
			stateHolder.Schedules[agentToSchedule].ScheduledDay(date).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}

		public SchedulingIslandDesktopTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerXXL76496, bool resourcePlannerHalfHourSkillTimeZon75509) : base(seperateWebRequest, resourcePlannerXXL76496, resourcePlannerHalfHourSkillTimeZon75509)
		{
		}
	}
}