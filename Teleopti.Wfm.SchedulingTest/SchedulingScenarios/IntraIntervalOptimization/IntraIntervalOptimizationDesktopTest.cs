using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntraIntervalOptimization
{
	[UseIocForFatClient]
	public class IntraIntervalOptimizationDesktopTest : IntraIntervalOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> StateHolder;

		[Test]
		public void ShouldSolveIntraIntervalIssue()
		{
			var date = new DateOnly(2017, 05, 05);
			var scenario = new Scenario("_");
			var activity = new Activity().WithId();
			var skill = new Skill().DefaultResolution(30).For(activity).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent1 = new Person().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date).WithId().InTimeZone(TimeZoneInfo.Utc);
			var agent2 = new Person().WithPersonPeriod(skill).WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass1 = new PersonAssignment(agent1, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 15)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 30)).ShiftCategory(shiftCategory);
			var stateHolder = StateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent1, agent2 }, new[] { ass1, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { OptimizationStepIntraInterval = true, ScheduleTag = new ScheduleTag() } };

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1 }, date.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent1].ScheduledDay(date).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}


		[TestCase(true)]
		[TestCase(false)]
		public void ShouldNotRemoveOvertimeLayers(bool teamblock)
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var activity = new Activity().WithId();
			var skill = new Skill().DefaultResolution(30).For(activity).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agent1 = new Person().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date).WithId().InTimeZone(TimeZoneInfo.Utc);
			var agent2 = new Person().WithPersonPeriod(skill).WithId().InTimeZone(TimeZoneInfo.Utc);
			var ass1 = new PersonAssignment(agent1, scenario, date).ShiftCategory(shiftCategory)
				.WithLayer(activity, new TimePeriod(8, 15, 16, 15))
				.WithOvertimeLayer(activity, new TimePeriod(16, 15, 16, 30));
			var ass2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 15, 16, 30)).ShiftCategory(shiftCategory);
			var stateHolder = StateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent1, agent2 }, new[] { ass1, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { OptimizationStepIntraInterval = true, ScheduleTag = new ScheduleTag() } };
			if (teamblock)
			{
				optimizationPreferences.Extra.UseTeamBlockOption = true;
				optimizationPreferences.Extra.UseBlockSameShiftCategory = true;
			}

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1 }, date.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent1].ScheduledDay(date).PersonAssignment().OvertimeActivities().Any()
				.Should().Be.True();
		}
	}
}