using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(true, true)]
	[TestFixture(false, true)]
	[TestFixture(true, false)]
	[TestFixture(false, false)]
	public class IntradayOptimizationDesktopMaxSeatTest : IConfigureToggleManager, ISetup
	{
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		private readonly bool _resourcePlannerMaxSeatsNew40939;
		private readonly bool _resourcePlannerSplitBigIslands42049;
		public IOptimizationCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public IInitMaxSeatForStateHolder InitMaxSeatForStateHolder;
		public IResourceCalculation ResourceOptimization;

		public IntradayOptimizationDesktopMaxSeatTest(bool resourcePlannerMaxSeatsNew40939, bool resourcePlannerSplitBigIslands42049)
		{
			_resourcePlannerMaxSeatsNew40939 = resourcePlannerMaxSeatsNew40939;
			_resourcePlannerSplitBigIslands42049 = resourcePlannerSplitBigIslands42049;
		}

		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.TeamHierarchy, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.TeamHierarchy, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.TeamNonHierarchy, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.TeamNonHierarchy, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.Block, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.Block, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.Classic, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.Classic, ExpectedResult = false)]
		public bool ShouldRespectMaxSeatWhenIntradayOptimizationIsMade(MaxSeatsFeatureOptions maxSeatsFeatureOptions, teamBlockStyle teamBlockStyle)
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site }.WithId();
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
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															new[] { skillDay, skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 1) }); //TODO - seems to be needed. Must be a bug I guess?
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = maxSeatsFeatureOptions },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true }
			};
			switch (teamBlockStyle)
			{
				case teamBlockStyle.TeamHierarchy:
					optPreferences.Extra.UseTeams = true;
					optPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy);
					break;
				case teamBlockStyle.TeamNonHierarchy:
					optPreferences.Extra.UseTeams = true;
					optPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag);
					break;
				case teamBlockStyle.Block:
					optPreferences.Extra.UseTeamBlockOption = true;
					optPreferences.Extra.BlockTypeValue = BlockFinderType.SchedulePeriod;
					optPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent);
					break;
				case teamBlockStyle.Classic:
					Assert.Ignore("To be fixed - 42592");
					optPreferences.Extra.UseTeamBlockOption = false;
					optPreferences.Extra.UseTeams = false;
					break;
			}

			InitMaxSeatForStateHolder.Execute(15);
			ResourceOptimization.ResourceCalculate(dateOnly, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, false, null, null);

			var wasGivenNewShift = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9);
			return wasGivenNewShift;
		}

		public enum teamBlockStyle
		{
			TeamHierarchy,
			TeamNonHierarchy,
			Block,
			Classic
		}

		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerMaxSeatsNew40939)
				toggleManager.Enable(Toggles.ResourcePlanner_MaxSeatsNew_40939);
			if (_resourcePlannerSplitBigIslands42049)
				toggleManager.Enable(Toggles.ResourcePlanner_SplitBigIslands_42049);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}