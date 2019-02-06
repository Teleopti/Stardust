using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationReduceUnnecessaryCallsTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public InitMaxSeatForStateHolder InitMaxSeatForStateHolder;
		public WorkShiftSelectorTrackWhatSkillDays WorkShiftSelector;

		[Test]
		public void ShouldNotUseMaxSeatSkillDaysFromOtherSite()
		{
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var siteNotInUse = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var teamNotInUse = new Team {Site = siteNotInUse }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = new Skill("skillet").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(16, 17), 10));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team, skill);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneDay(dateOnly);
			var agentNotInUse = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, teamNotInUse, skill).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).WithLayer(activity, new TimePeriod(16, 17)).ShiftCategory(shiftCategory);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var assNotInUse = new PersonAssignment(agentNotInUse, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour, agentNotInUse},
															new[] { ass, assOneHour, assNotInUse },
															new[] { skillDay, skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 1) }); //TODO - seems to be needed. Must be a bug I guess?
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};
			InitMaxSeatForStateHolder.Execute(15); //causes maxskills from other sites to be included in stateholder

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			var maxSeatSkillDaysUsedOnCurrentDate = WorkShiftSelector.SkillDaysAsParameter.Where(x => x.Skill is MaxSeatSkill && x.CurrentDate == dateOnly);
			maxSeatSkillDaysUsedOnCurrentDate.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotUseSameMaxSeatSkillDayEvenIfInitMaxSeatForStateHolderIsUsed()
		{
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = new Skill("skillet").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(16, 17), 10));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team, skill);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).WithLayer(activity, new TimePeriod(16, 17));
			assOneHour.SetShiftCategory(shiftCategory);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			ass.SetShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															new[] { skillDay, skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 1) }); //TODO - seems to be needed. Must be a bug I guess?
			var optPreferences = new OptimizationPreferences
			{
				Advanced = {UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats},
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true},
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};
			InitMaxSeatForStateHolder.Execute(15); //causes maxskills to be included in stateholder

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			var maxSeatSkillDaysUsedOnCurrentDate = WorkShiftSelector.SkillDaysAsParameter.Where(x => x.Skill is MaxSeatSkill && x.CurrentDate == dateOnly);
			maxSeatSkillDaysUsedOnCurrentDate.Count().Should().Be.EqualTo(1);
		}

		public override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<WorkShiftSelectorTrackWhatSkillDays>().For<WorkShiftSelectorForMaxSeat>();
		}
	}
}