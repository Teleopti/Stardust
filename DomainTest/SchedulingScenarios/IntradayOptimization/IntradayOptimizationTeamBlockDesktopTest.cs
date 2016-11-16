using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTestWithStaticDependenciesAvoidUse]
	[TestFixture(true)]
	[TestFixture(false)]
	public class IntradayOptimizationTeamBlockDesktopTest : IConfigureToggleManager
	{
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		private readonly bool _resourcePlannerMaxSeatsNew40939;
		public IOptimizationCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public IInitMaxSeatForStateHolder InitMaxSeatForStateHolder;
		public IResourceOptimization ResourceOptimization;

		public IntradayOptimizationTeamBlockDesktopTest(bool resourcePlannerMaxSeatsNew40939)
		{
			_resourcePlannerMaxSeatsNew40939 = resourcePlannerMaxSeatsNew40939;
		}

		[Test]
		public void ShouldNotCrashWhenUsingKeepExistingDaysOff()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId())));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepShiftsWithinDay = true;
			optimizationPreferences.Extra.UseTeams = true;
			var daysOffPreferences = new DaysOffPreferences { UseKeepExistingDaysOff = true };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(null,
					new NoSchedulingProgress(),
					schedulerStateHolderFrom,
					new List<IScheduleDay> { schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly) },
					null,
					null,
					optimizationPreferences,
					false,
					daysOffPreferences,
					new FixedDayOffOptimizationPreferenceProvider(daysOffPreferences));
			});
		}

		[Test]
		public void ShouldMarkDayToBeRecalculated()
		{
			if(!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site }.WithId();
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentScheduledOneHour.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team));
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly);
			assOneHour.AddActivity(activity, new TimePeriod(16, 17));
			assOneHour.SetShiftCategory(new ShiftCategory("_"));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(9, 17));
			ass.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = {UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats},
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false},
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, null, null, optPreferences, false, null, null);

			stateHolder.DaysToRecalculate
				.Should().Have.SameValuesAs(dateOnly);
		}

		[Test]
		public void ShouldNotMarkDayThatIsNotChangedToBeRecalculated()
		{
			if (!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site }.WithId();
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentScheduledOneHour.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team));
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly);
			assOneHour.AddActivity(activity, new TimePeriod(16, 17));
			assOneHour.SetShiftCategory(new ShiftCategory("_"));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(9, 17));
			ass.SetShiftCategory(new ShiftCategory("_"));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, null, null, optPreferences, false, null, null);

			stateHolder.DaysToRecalculate
				.Should().Be.Empty();
		}

		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.TeamHierarchy, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.TeamHierarchy, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.TeamNonHierarchy, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.TeamNonHierarchy, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, teamBlockStyle.Block, ExpectedResult = true)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, teamBlockStyle.Block, ExpectedResult = false)]
		public bool ShouldRespectMaxSeatWhenIntradayOptimizationIsMade(MaxSeatsFeatureOptions maxSeatsFeatureOptions, teamBlockStyle teamBlockStyle)
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site }.WithId();
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var skill = new Skill("skillet", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(16, 17), 10));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentScheduledOneHour.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team), skill);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly);
			assOneHour.AddActivity(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 16));
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
			}

			InitMaxSeatForStateHolder.Execute(15);
			ResourceOptimization.ResourceCalculate(dateOnly, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, null, null, optPreferences, false, null, null);

			var wasGivenNewShift = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9);
			return wasGivenNewShift;
		}

		public enum teamBlockStyle
		{
			TeamHierarchy,
			TeamNonHierarchy,
			Block
		}

		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMaxSeatsNew40939)
				toggleManager.Enable(Toggles.ResourcePlanner_MaxSeatsNew_40939);
		}
	}
}