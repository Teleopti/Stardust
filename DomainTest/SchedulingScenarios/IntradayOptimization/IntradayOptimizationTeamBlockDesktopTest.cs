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
using Teleopti.Ccc.TestCommon.Scheduling;


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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences
				{
					ScheduleTag = NullScheduleTag.Instance,
					OptimizationStepDaysOff = true,
					OptimizationStepShiftsWithinDay = true
				},
				Extra = {UseTeams = true}
			};
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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

		[Test]
		public void ShouldConsiderCrossSkillAgents()
		{
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var scenario = new Scenario();
			var activity = new Activity();
			var dateOnly = new DateOnly(2010, 1, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromHours(1)));
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen(new TimePeriod(8, 16));
			var skillC = new Skill("C").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen(new TimePeriod(9, 17));
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneWeek(dateOnly);
			var agentBC = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillB, skillC).WithSchedulePeriodOneWeek(dateOnly);
			var assAB = new PersonAssignment(agentAB, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16)); //0.5 resources on skillB
			var assBC = new PersonAssignment(agentBC, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(7, 15)); //should be moved to 9-17 
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] {agentAB, agentBC}, new[] {assAB, assBC}, new[]
				{
					skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1),
					skillB.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), 1.1)), //make test red if skillA/agentAB isn't counted
					skillC.CreateSkillDayWithDemand(scenario, dateOnly, 1)
				});
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) },
				Shifts = { KeepShiftCategories = true}
			};

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agentBC }, dateOnly.ToDateOnlyPeriod(), optPreferences, null);

			schedulerStateHolderFrom.Schedules[agentBC].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(9);
		}
		
		[Test]
		public void ShouldNotCrashOnSelectionOverMultipleSchedulePeriodsUsingBlockAndActivityPreferences()
		{
			var date = new DateOnly(2017, 11, 27);
			var scenario = new Scenario().WithId();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activity = new Activity().WithId();
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneWeek(new DateOnly(2017, 11, 23));
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));			
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), agent, ass, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true, BlockTypeValue = BlockFinderType.BetweenDayOff},
				Shifts = {KeepActivityLength = true, ActivityToKeepLengthOn = activity}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] {agent}, new DateOnlyPeriod(date, date.AddDays(4)), optimizationPreferences,
					new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			});
		}
		
		
		[Test]
		public void ShouldIndividualFlexableWhenNotBlock()
		{
			var dateOnly = new DateOnly(2017, 9, 25);
			var activity = ActivityFactory.CreateActivity("phone");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario().WithId();
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(60), TimeSpan.FromHours(11), TimeSpan.FromHours(8));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(0), NegativeDayOffTolerance = 3 };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(dateOnly, new RuleSetBag(ruleSet), contract, 
				ContractScheduleFactory.Create7DaysWorkingContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDays = new List<ISkillDay>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				skillDays.Add(
					i == 6
						? skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(9, TimeSpan.FromMinutes(180)))
						: skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(180)))
				);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(9, 0, 17, 0)).WithId());
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), agent, asses, skillDays);
			var optPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences {ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true}
			};
			
			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), optPreferences,
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			
			for (var i = 0; i < 7; i++)
			{
				var date = dateOnly.AddDays(i);
				var dateTime1 = TimeZoneHelper.ConvertToUtc(date.Date, agent.PermissionInformation.DefaultTimeZone());
				stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().Period
					.Should()
					.Be.EqualTo(i == 6
						? new DateTimePeriod(dateTime1.AddHours(9), dateTime1.AddHours(17))
						: new DateTimePeriod(dateTime1.AddHours(10), dateTime1.AddHours(18)));
			}
		}

		[Test]
		public void ShouldUseSameShiftWhenBlock()
		{
			var dateOnly = new DateOnly(2017, 9, 25);
			var activity = ActivityFactory.CreateActivity("phone");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario().WithId();
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(60), TimeSpan.FromHours(11), TimeSpan.FromHours(8));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(0), NegativeDayOffTolerance = 3 };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(dateOnly, new RuleSetBag(ruleSet), contract, 
				ContractScheduleFactory.Create7DaysWorkingContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDays = new List<ISkillDay>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				skillDays.Add(
					i == 6
						? skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(9, TimeSpan.FromMinutes(180)))
						: skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(180)))
				);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(9, 0, 17, 0)).WithId());
			}
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShift = true, BlockTypeValue = BlockFinderType.SchedulePeriod }
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), agent, asses, skillDays);
			
			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), optimizationPreferences,
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			for (var i = 0; i < 7; i++)
			{
				var date = dateOnly.AddDays(i);
				var dateTime1 = TimeZoneHelper.ConvertToUtc(date.Date, agent.PermissionInformation.DefaultTimeZone());
				stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().Period
					.Should().Be.EqualTo(new DateTimePeriod(dateTime1.AddHours(10), dateTime1.AddHours(18)));
			}
		}

		[Test]
		public void ShouldHandleAgentStartingInTheMiddleOfTheSchedulePeriod()
		{
			var date = new DateOnly(2017, 9, 25);
			var activity = new Activity();
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario().WithId();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(date.AddDays(2), new RuleSetBag(ruleSet), new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(date);
			var skillDays = new List<ISkillDay>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				skillDays.Add(skill.CreateSkillDayWithDemandPerHour(scenario, date.AddDays(i), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(180))));
				asses.Add(new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 0)).WithId());
			}
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true, BlockTypeValue = BlockFinderType.BetweenDayOff }
			};
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), agent, asses, skillDays);

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), optimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			for (var i = 2; i < 7; i++)
			{
				stateHolder.Schedules[agent].ScheduledDay(date.AddDays(i)).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(10);
			}
		}
	}
}