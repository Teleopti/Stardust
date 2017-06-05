using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
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
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayOptimizationReduceUnnecessaryCallsTest : ISetup
	{
		public OptimizationExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public InitMaxSeatForStateHolder InitMaxSeatForStateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public WorkShiftSelectorTrackWhatSkillDays WorkShiftSelector;

		[Test]
		public void ShouldNotUseMaxSeatSkillDaysFromOtherSite()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var siteNotInUse = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var teamNotInUse = new Team {Site = siteNotInUse }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = new Skill("skillet").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(new TimePeriod(16, 17), 10));
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
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = new Skill("skillet").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(new TimePeriod(16, 17), 10));
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
			system.UseTestDouble<WorkShiftSelectorTrackWhatSkillDays>().For<WorkShiftSelectorForMaxSeat>();
		}
	}
}