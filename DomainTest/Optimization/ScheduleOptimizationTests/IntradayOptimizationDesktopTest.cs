using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
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
		public void ShouldNotPlaceSameShiftForAllAgentsInIsland()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), shiftCategory));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16))};
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(15, TimeSpan.FromMinutes(90)));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			var schedules = new List<IScheduleDay>();
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
				schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
				schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
				scheduleDay.AddMainShift(ass);
				schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
				schedules.Add(scheduleDay);
			}
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			Target.Optimize(schedules, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			var uniqueSchedulePeriods = new HashSet<DateTimePeriod>();
			foreach (var oldSchedule in schedules)
			{ 
				uniqueSchedulePeriods.Add(schedulerStateHolderFrom.Schedules[oldSchedule.Person].ScheduledDay(dateOnly).PersonAssignment().Period);
			}
			uniqueSchedulePeriods.Count.Should().Be.EqualTo(2); 
		}

		[Test]
		public void ShouldWorkWhenScenarioIsNotDefault()
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
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);


			Target.Optimize(new[] {scheduleDay}, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());
			
			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldWorkWhenScenarioIsDefault()
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
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldConsiderAbsences()
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
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			var absence = AbsenceFactory.CreateAbsence("absence");
			absence.InContractTime = true;
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9));
			scheduleDay.CreateAndAddAbsence(absenceLayer);
			
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);	
		}

		[Test]
		public void ShouldConsiderMeetings()
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
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);

			var meetingPerson = new MeetingPerson(agent, false);
			var period = new DateTimePeriod(2010, 1, 1, 7, 2010, 1, 1, 8);
			var meeting = new Meeting(agent, new List<IMeetingPerson> { meetingPerson }, "subject", "location", "description", phoneActivity, scenario);
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);

			((ScheduleRange) schedulerStateHolderFrom.Schedules[agent]).Add(personMeeting);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);


			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
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
			agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory){MaxNumberOf = 1});

			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());

			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1));
			ass2.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass2.SetShiftCategory(shiftCategory);
			
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			var scheduleDay2 = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly.AddDays(1));
			scheduleDay2.AddMainShift(ass2);

			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay2);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(new[] { scheduleDay }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
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

			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			var restriction = new RotationRestriction() {ShiftCategory = shiftCategory};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			((ScheduleRange)schedulerStateHolderFrom.Schedules[agent]).Add(personRestriction);

			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseRotations = true;
			optimizationPreferences.General.RotationsValue = 1.0d;

			Target.Optimize(new[] { scheduleDay }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
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

			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			var restriction = new AvailabilityRestriction() {NotAvailable = true};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			((ScheduleRange)schedulerStateHolderFrom.Schedules[agent]).Add(personRestriction);

			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseAvailabilities = true;
			optimizationPreferences.General.AvailabilitiesValue = 1.0d;

			Target.Optimize(new[] { scheduleDay }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
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

			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);

			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);

			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			var preferenceRestriction = new PreferenceRestriction() {ShiftCategory = shiftCategory};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			((ScheduleRange)schedulerStateHolderFrom.Schedules[agent]).Add(preferenceDay);

			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UsePreferences = true;
			optimizationPreferences.General.PreferencesValue = 1.0d;

			Target.Optimize(new[] { scheduleDay }, optimizationPreferences, new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
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
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.SetUndoRedoContainer(undoRedoContainer);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			undoRedoContainer.CreateBatch("for testing to have one batch to undo below. See if we can put this in optimization code later.");
			Target.Optimize(new[] { scheduleDay }, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());
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
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 01, 11);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(10, 15, 10, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
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
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2016, 1, 10, 2016, 1, 20)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { {skill, new List<ISkillDay> {skillDay}}};
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			var scheduleDays = new List<IScheduleDay>();
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
				ass.SetShiftCategory(shiftCategory);

				var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly.AddDays(i));
				scheduleDay.AddMainShift(ass);
				schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
				scheduleDays.Add(scheduleDay);
			}

			Target.Optimize(scheduleDays, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
									 .Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldNotCrashWithNoPermission()
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
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom();
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.AllPermittedPersons.Add(agent);
			schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Add(agent);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedulerStateHolderFrom.SchedulingResultState.Schedules, agent, dateOnly);
			scheduleDay.AddMainShift(ass);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.Modify(scheduleDay);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } };
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);

			using (new CustomAuthorizationContext(new missingPermissionsOnAgentsOverlappingSchedulePeriod()))
			{
				Assert.DoesNotThrow(
				() =>
					Target.Optimize(new[] { scheduleDay }, new OptimizationPreferencesDefaultValueProvider().Fetch(),
						new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()),
						new NoSchedulingProgress()));
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
			const int numberOfIslands = 20;
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
			schedulerStateHolderFrom.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(dateOnly, dateOnly), TimeZoneInfo.Utc);
			schedulerStateHolderFrom.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2009, 12, 31, 2010, 1, 2)));
			schedulerStateHolderFrom.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			var agents = new List<IPerson>();
			var scheduleDays = new List<IScheduleDay>();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
				WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
				var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
				schedulerStateHolderFrom.SchedulingResultState.SkillDays[skill] = new List<ISkillDay> { skillDay };

				var agent = new Person().WithId();
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
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
				Target.Optimize(scheduleDays, new OptimizationPreferencesDefaultValueProvider().Fetch(), new DateOnlyPeriod(dateOnly, dateOnly), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoSchedulingProgress());
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