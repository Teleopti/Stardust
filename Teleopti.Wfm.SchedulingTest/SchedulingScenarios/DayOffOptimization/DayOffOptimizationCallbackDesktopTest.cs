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
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationCallbackDesktopTest : DayOffOptimizationScenario
	{
		public DayOffOptimizationDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
	
		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5, 25, 5);	
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}};

			var callbackTracker = new TrackOptimizationCallback();
			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), callbackTracker);
			callbackTracker.SuccessfulOptimizations()
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldBeAbleToCancel()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5, 25, 5);	
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}};

			var callbackTracker = new TrackOptimizationCallback();
			callbackTracker.SetCancelled();
			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), callbackTracker);
			callbackTracker.SuccessfulOptimizations()
				.Should().Be.EqualTo(0);
			callbackTracker.UnSuccessfulOptimizations()
				.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void ShouldDoUnsuccesfulCallbacks()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 5, 5, 5, 5, 5, 5);	
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}};

			var callbackTracker = new TrackOptimizationCallback();
			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), callbackTracker);
			callbackTracker.UnSuccessfulOptimizations()
				.Should().Be.EqualTo(1);
		}

		public DayOffOptimizationCallbackDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}