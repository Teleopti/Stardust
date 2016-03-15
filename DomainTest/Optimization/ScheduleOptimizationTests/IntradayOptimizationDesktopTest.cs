using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_IntradayIslands_36939)]
	[Toggle(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049)]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationDesktopTest : ISetup
	{
		public IOptimizeIntradayDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldSynchronizeScheduleStateHolder()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010,1,1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = SkillFactory.CreateSkill("_").WithId();
			skill.Activity = phoneActivity;
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2020,1,1)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };

			Target.Optimize(new[] {scheduleDay}, new OptimizationPreferences(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());
			
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldSynchronizeScheduleStateHolderInDefaultScenario()
		{
			var scenario = new Scenario("_") {DefaultScenario = true};
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = SkillFactory.CreateSkill("_").WithId();
			skill.Activity = phoneActivity;
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };

			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferences(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test, Ignore]
		public void ShouldBeAbleToUndo()
		{
			var undoRedoContainer = new UndoRedoContainer(int.MaxValue);

			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = SkillFactory.CreateSkill("_").WithId();
			skill.Activity = phoneActivity;
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2010, 1, 1, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.SchedulingResultState.Schedules.SetUndoRedoContainer(undoRedoContainer);

			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferences(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			undoRedoContainer.UndoAll();
			
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(0);
		}

		[Test, Explicit]
		public void ShouldNotCrashIfALotOfThreads()
		{
			const int numberOfIslands = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)));
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			var agents = new List<IPerson>();
			var scheduleDays = new List<IScheduleDay>();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = SkillFactory.CreateSkill("_").WithId();
				skill.Activity = phoneActivity;
				WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
				var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skill] = new List<ISkillDay> { skillDay };

				var agent = new Person().WithId();
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
				agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
				agents.Add(agent);

				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
				scheduleDays.Add(scheduleDay);
			}
			agents.ForEach(x => schedulerStateHolderFrom.AllPermittedPersons.Add(x));

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(scheduleDays, new OptimizationPreferences(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());
			});
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FillSchedulerStateHolderFromRam>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult>();
		}
	}
}