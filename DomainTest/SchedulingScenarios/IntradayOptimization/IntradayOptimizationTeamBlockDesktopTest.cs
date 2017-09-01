using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationTeamBlockDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeRuleSetBagRepository RuleSetBagRepository;

		[Test]
		public void ShouldNotCrashWhenUsingKeepExistingDaysOff()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepShiftsWithinDay = true;
			optimizationPreferences.Extra.UseTeams = true;
			var daysOffPreferences = new DaysOffPreferences { UseKeepExistingDaysOff = true };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(),
					schedulerStateHolderFrom,
					new[] { agent }, dateOnly.ToDateOnlyPeriod(),
					optimizationPreferences,
					new FixedDayOffOptimizationPreferenceProvider(daysOffPreferences));
			});
		}

		[Test]
		public void ShouldMarkDayToBeRecalculated()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
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

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Have.SameValuesAs(dateOnly);
		}

		[Test]
		public void ShouldNotMarkDayThatIsNotChangedToBeRecalculated()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
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

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldMarkDayToBeRecalculatedWhenDoNotBreak()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Have.SameValuesAs(dateOnly);
		}

		[Test]
		public void ShouldNotMarkDayToBeRecalculatedWhenDoNotBreak()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent },
															new[] { ass },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldConsiderKeepStartTime()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) },
				Shifts = {KeepStartTimes = true}
			};

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(8);
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldScheduleTeam()
		{
			var scenario = new Scenario("_");
			var activity = new Activity();
			var dateOnly = new DateOnly(2010, 1, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(3, 0, 3, 0, 15), new TimePeriodWithSegment(11, 0, 11, 0, 15), shiftCategory));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(11, 0, 11, 0, 15), new TimePeriodWithSegment(19, 0, 19, 0, 15), shiftCategory));
			var bag = new RuleSetBag(ruleSet1, ruleSet2) { Description = new Description("_")}.WithId();
			RuleSetBagRepository.Add(bag);
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(bag, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(dateOnly);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(bag, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(7, 16));
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(activity, new TimePeriod(7, 16));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent1, agent2}, new[] { ass1, ass2}, skillDay);
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag), UseTeamSameStartTime = true }
			};

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent1, agent2 }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			var newAss1 = schedulerStateHolderFrom.Schedules[agent1].ScheduledDay(dateOnly).PersonAssignment();
			var newAss2 = schedulerStateHolderFrom.Schedules[agent2].ScheduledDay(dateOnly).PersonAssignment();
			newAss1.Period.Should().Be.EqualTo(newAss2.Period);
			newAss1.ShiftCategory.Should().Be.EqualTo(shiftCategory);
		}

		[Test]
		public void ShouldNotCrashOnSchedulePeriodSameShiftCategoryAndKeepActivityLength()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2014, 4, 1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new ContractWithMaximumTolerance();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(2);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				asses.Add(ass);
				if (i == 5 || i == 6)
				{
					ass.SetDayOff(new DayOffTemplate());
				}
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 1, 1);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true, BlockTypeValue = BlockFinderType.SchedulePeriod},
				Shifts = {KeepActivityLength = true, ActivityToKeepLengthOn = new Activity("_")}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] {agent}, period, optimizationPreferences,
					new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			});
		}

		[Test]
		public void ShouldNotCrashWhenAgentHaveNoSkill()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2014, 4, 1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new ContractWithMaximumTolerance();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(2);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				asses.Add(ass);
				if (i == 5 || i == 6)
				{
					ass.SetDayOff(new DayOffTemplate());
				}
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 1, 1);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true, BlockTypeValue = BlockFinderType.BetweenDayOff }	
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, period, optimizationPreferences,
					new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			});
		}

		public IntradayOptimizationTeamBlockDesktopTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, bool resourcePlannerSpeedUpShiftsWithinDay45694) : base(resourcePlannerMergeTeamblockClassicIntraday45508, resourcePlannerSpeedUpShiftsWithinDay45694)
		{
		}
	}
}