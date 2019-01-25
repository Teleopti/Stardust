using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
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
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationDesktopMaxSeatTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			assOneHour.SetShiftCategory(shiftCategory); //shouldn't be needed I think
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 16));
			ass.SetShiftCategory(shiftCategory); //shouldn't be needed I think
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															new[] { skillDay});
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
					optPreferences.Extra.UseTeamBlockOption = false;
					optPreferences.Extra.UseTeams = false;
					break;
			}

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

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
	}
}