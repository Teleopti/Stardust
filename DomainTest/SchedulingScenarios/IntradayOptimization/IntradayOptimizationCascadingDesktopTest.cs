using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	[ShareLogonOnThreads]
	public class IntradayOptimizationCascadingDesktopTest : ISetup
	{
		public OptimizeIntradayIslandsDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public IResourceCalculationContextFactory ResourceCalculationContextFactory;
		public IFullResourceCalculation FullResourceCalculation;

		[Test]
		public void ShouldShovelResources()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skillA = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 17, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 17, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, new[] { skillDayA, skillDayB });

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldKeepCalculatedLoggedOnValueWhenHavingOuterResContext()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 17, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 17, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAB.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentAB.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var agentB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentB.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillB });
			agentB.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var assAB = new PersonAssignment(agentAB, scenario, date);
			assAB.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var assB = new PersonAssignment(agentB, scenario, date);
			assB.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentB }, new[] { assAB, assB }, new[] { skillDayA, skillDayB });

			using (ResourceCalculationContextFactory.Create(schedulerStateHolderFrom.Schedules, schedulerStateHolderFrom.SchedulingResultState.Skills))
			{
				FullResourceCalculation.Execute();

				Target.Optimize(new[] { agentAB, agentB }, new DateOnlyPeriod(date, date), new OptimizationPreferencesDefaultValueProvider().Fetch(), null);

				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
					.Should().Be.EqualTo(1);
				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
					.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldKeepCalculatedLoggedOnValueWhenHavingNoOuterResContext()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 17, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 17, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAB.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentAB.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var agentB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentB.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillB });
			agentB.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var assAB = new PersonAssignment(agentAB, scenario, date);
			assAB.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var assB = new PersonAssignment(agentB, scenario, date);
			assB.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentB }, new[] { assAB, assB }, new[] { skillDayA, skillDayB });

			FullResourceCalculation.Execute();

			Target.Optimize(new[] { agentAB, agentB }, new DateOnlyPeriod(date, date), new OptimizationPreferencesDefaultValueProvider().Fetch(), null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
				.Should().Be.EqualTo(1);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().CalculatedLoggedOn
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void IncreasedResourcesShouldBeSameAsDecreasedInOtherSkills_WhenResourceCalcHasBeenMadeBeforeOptimization()
		{
			var scenario = new Scenario("_");
			var activity = ActivityFactory.CreateActivity("_");
			var date = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)), PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 17, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 17, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			var skillC = new Skill("C", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillC.SetCascadingIndex(3);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillC, new TimePeriod(8, 0, 17, 0));
			var skillDayC = skillC.CreateSkillDayWithDemand(scenario, date, 1);
			var agentAB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAB.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agentAB.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var agentAC = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentAC.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillC });
			agentAC.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			var assAB = new PersonAssignment(agentAB, scenario, date);
			assAB.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var assAC = new PersonAssignment(agentAC, scenario, date);
			assAC.AddActivity(activity, new TimePeriod(8, 0, 17, 0));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agentAB, agentAC }, new[] { assAB, assAC }, new[] { skillDayA, skillDayB, skillDayC });

			FullResourceCalculation.Execute();

			Target.Optimize(new[] { agentAB, agentAC }, new DateOnlyPeriod(date, date), new OptimizationPreferencesDefaultValueProvider().Fetch(), null);

			var skillAShovledResources = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().ResourceLoggonOnDiff();
			var skillBShovledResources = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().ResourceLoggonOnDiff();
			var skillCShovledResources = schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillC].Single().SkillStaffPeriodCollection.First().ResourceLoggonOnDiff();
			(skillAShovledResources + skillBShovledResources + skillCShovledResources).IsZero()
				.Should().Be.True();
		}


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}