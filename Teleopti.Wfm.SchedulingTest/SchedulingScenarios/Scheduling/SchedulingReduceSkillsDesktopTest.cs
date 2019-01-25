using System;
using System.Linq;
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

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingReduceSkillsDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public ReduceIslandsLimits ReduceIslandsLimits;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		
		[TestCase(0, 12, 12, 24, true)]
		[TestCase(8, 16, 8, 16, true)]
		[TestCase(0, 12, 13, 24, false)]
		[TestCase(0, 12, 0, 12, false)]
		public void ShouldHandleDifferentOpenHoursWhenReducingSkills(int skill1Start, int skill1End, int skill2Start, int skill2End, bool canBeScheduled)
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			var scenario = new Scenario();
			var activity = new Activity{RequiresSkill = true}.WithId();
			var skillA = new Skill().For(activity).IsOpenBetween(skill1Start, skill1End).WithId();
			var skillB = new Skill().For(activity).IsOpenBetween(skill2Start, skill2End).WithId();
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
			
			stateHolder.Schedules[agentToSchedule].ScheduledDay(date).PersonAssignment(true).ShiftLayers.Any()
				.Should().Be.EqualTo(canBeScheduled);
		}

		[Test]
		public void ShouldNotPlaceShiftOnReducedSkill()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);
			var scenario = new Scenario();
			var activity = new Activity{RequiresSkill = true}.WithId();
			var skillA = new Skill().For(activity).IsOpenBetween(11, 12).WithId();
			var skillB = new Skill().For(activity).IsOpenBetween(12, 13).WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(11, 0, 12, 0, 60), new TimePeriodWithSegment(12, 0, 13, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(1), OperatorLimiter.Equals));
			var date = new DateOnly(2015, 10, 12);
			var agentToSchedule = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date);
			var otherAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA);
			var skillDays = new[]
			{
				skillA.CreateSkillDayWithDemand(scenario, date, 1000),
				skillB.CreateSkillDayWithDemand(scenario, date, 0.01)
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, date, new[] { agentToSchedule, otherAgent }, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agentToSchedule}, date.ToDateOnlyPeriod());
			
			stateHolder.Schedules[agentToSchedule].ScheduledDay(date).PersonAssignment(true).ShiftLayers.Single().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(12));
		}
		

		public SchedulingReduceSkillsDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}