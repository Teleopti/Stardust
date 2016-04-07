using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[TestFixture(false, false)]
	[TestFixture(false, true)]
	[TestFixture(true, false)]
	[TestFixture(true, true)]
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	public class IntradayOptimizationIslandDesktopTest : IntradayOptimizationScenario, ISetup
	{
		public IOptimizeIntradayDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		public IntradayOptimizationIslandDesktopTest(bool skillGroupDeleteAfterCalculation, bool jumpOutWhenLargeGroupIsHalfOptimized) 
			: base(skillGroupDeleteAfterCalculation, true, jumpOutWhenLargeGroupIsHalfOptimized)
		{
		}

		[Test]
		public void ShouldNotPlaceSameShiftForAllAgentsInIsland()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16))};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(15, TimeSpan.FromMinutes(90)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId();
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
				agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDay);

			Target.Optimize(schedulerStateHolderFrom.Schedules.SchedulesForDay(dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			var uniqueSchedulePeriods = new HashSet<DateTimePeriod>();
			foreach (var oldSchedule in schedulerStateHolderFrom.Schedules.SchedulesForDay(dateOnly))
			{ 
				uniqueSchedulePeriods.Add(schedulerStateHolderFrom.Schedules[oldSchedule.Person].ScheduledDay(dateOnly).PersonAssignment().Period);
			}
			uniqueSchedulePeriods.Count.Should().Be.EqualTo(2); 
		}

		[Test]
		public void ShouldConsiderAgentsNotPartOfAllPermittedPersons()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(15, TimeSpan.FromMinutes(65)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId();
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
				agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDay);
			var oneAgent = asses.First().Person;
			asses.Select(x => x.Person).Where(x => x != oneAgent).ForEach(x => schedulerStateHolderFrom.AllPermittedPersons.Remove(x));

			Target.Optimize(new[] { schedulerStateHolderFrom.Schedules[oneAgent].ScheduledDay(dateOnly)}, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[oneAgent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Hour
				.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldWorkWhenScenarioIsNotDefault()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010,1,1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new [] {agent}, new[] {ass}, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);
			
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldSetScheduleTag()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var scheduleTag = new ScheduleTag();
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = scheduleTag;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, optPref, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(scheduleTag);
		}

		[Test]
		public void ShouldSetNullScheduleTag()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = NullScheduleTag.Instance;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, 
				new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, new ScheduleTag()) }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, optPref, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(NullScheduleTag.Instance);
		}

		[Test]
		public void ShouldKeepScheduleTag()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = KeepOriginalScheduleTag.Instance;
			var currScheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent },
				new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, currScheduleTag) }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, optPref, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(currScheduleTag);
		}

		[Test]
		public void ShouldNotSetScheduleTagForNonModifiedAssignment()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(9, 15, 9, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, optPref, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(NullScheduleTag.Instance);
		}

		[Test]
		public void ShouldNotTouchAssignmentsForAgentsNotOptimized()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var agent2 = new Person().WithId();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent2.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent2.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent, agent2 }, new[] { ass, ass2 }, skillDay);
			schedulerStateHolderFrom.Schedules.TakeSnapshot();

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules.DifferenceSinceSnapshot().Count()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldWorkWhenScenarioIsDefault()
		{
			var scenario = new Scenario("_") {DefaultScenario = true};
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldNotDuplicateScheduleData()
		{
			var scenario = new Scenario("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = new Activity("_"), TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			var meetingPerson = new MeetingPerson(agent, false);
			var meeting = new Meeting(agent, new List<IMeetingPerson> { meetingPerson }, "subject", "location", "description", new Activity("_"), scenario);
			var personMeeting = new PersonMeeting(meeting, meetingPerson, new DateTimePeriod(2010, 1, 1, 2010, 1, 2));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, 
				new IScheduleData[]
				{
					new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9))),
					personMeeting,
					new PreferenceDay(agent, dateOnly, new PreferenceRestriction()), 
				}, skillDay);
		
			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAbsenceCollection().Count.Should().Be.EqualTo(1);
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonRestrictionCollection().Count.Should().Be.EqualTo(1);
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonMeetingCollection().Count.Should().Be.EqualTo(1);
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersistableScheduleDataCollection().OfType<IPreferenceDay>().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldConsiderAbsences()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());

			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence {InContractTime = true}, new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9)));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent}, new IScheduleData[] {ass, personAbsence}, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderMeetings()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());

			var meetingPerson = new MeetingPerson(agent, false);
			var period = new DateTimePeriod(2010, 1, 1, 7, 2010, 1, 1, 8);
			var meeting = new Meeting(agent, new List<IMeetingPerson> { meetingPerson }, "subject", "location", "description", phoneActivity, scenario);
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personMeeting }, skillDay);

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderShiftCategoryLimitations()
		{
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
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) {MaxNumberOf = 1});
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			ass2.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass2.SetShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agent }, new IScheduleData[] { ass, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderRotations()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(shiftCategory);
			var restriction = new RotationRestriction() {ShiftCategory = shiftCategory};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseRotations = true;
			optimizationPreferences.General.RotationsValue = 1.0d;

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderAvailabilities()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(shiftCategory);
			var restriction = new AvailabilityRestriction() {NotAvailable = true};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseAvailabilities = true;
			optimizationPreferences.General.AvailabilitiesValue = 1.0d;

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderPreferences()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(shiftCategory);
			var preferenceRestriction = new PreferenceRestriction() {ShiftCategory = shiftCategory};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UsePreferences = true;
			optimizationPreferences.General.PreferencesValue = 1.0d;

			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(stateHolder.SchedulingResultState.Schedules, agent, dateOnly) }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToUndo()
		{
			var undoRedoContainer = new UndoRedoContainer(int.MaxValue);

			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.SetUndoRedoContainer(undoRedoContainer);

			undoRedoContainer.CreateBatch("for testing to have one batch to undo below. See if we can put this in optimization code later.");
			Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);
			undoRedoContainer.CommitBatch();
			undoRedoContainer.Undo();
		
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldWorkWithSchedulesOutsideOptimizationPeriod()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016, 01, 11);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(10, 15, 10, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(100), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(63), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				TimePeriod shiftPeriod;
				switch (i)
				{
					case 0:
						shiftPeriod=new TimePeriod(10,0,17,0);
						break;
					case 1:
						shiftPeriod=new TimePeriod(8,0,17,0);
						break;
					default:
						shiftPeriod=new TimePeriod(9,0,17,0);
						break;
				}
				var ass = new PersonAssignment(agent, scenario, dateOnly.AddDays(i));
				ass.AddActivity(phoneActivity, shiftPeriod);
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)), new[] { agent }, asses, skillDay);

			Target.Optimize(schedulerStateHolderFrom.Schedules.SchedulesForPeriod(new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)), new[] {agent}), new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
									 .Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldNotCrashWithNoPermission()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			using (new CustomAuthorizationContext(new missingPermissionsOnAgentsOverlappingSchedulePeriod()))
			{
				Assert.DoesNotThrow(
				() =>
					Target.Optimize(new[] { ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly) }, new OptimizationPreferencesDefaultValueProvider().Fetch(),
						new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null));
			}
		}
		private class missingPermissionsOnAgentsOverlappingSchedulePeriod : PrincipalAuthorizationWithFullPermission
		{
			public override IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
			{
				return new[] { new DateOnlyPeriod(1800, 1, 1, 1801, 1, 1) };
			}
		}

		[Test]
		public void ShouldNotCrashIfLotsOfIslands()
		{
			const int numberOfIslands = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var asses = new List<IPersonAssignment>();
			var skillDays = new List<ISkillDay>();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
				WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
				var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
				skillDays.Add(skillDay);
				var agent = new Person().WithId();
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
				agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDays);

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(stateHolder.Schedules.SchedulesForDay(dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), null);
			});
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
			// share the same current principal on all threads
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(Thread.CurrentPrincipal as ITeleoptiPrincipal)).For<ICurrentTeleoptiPrincipal>();
		}
	}
}