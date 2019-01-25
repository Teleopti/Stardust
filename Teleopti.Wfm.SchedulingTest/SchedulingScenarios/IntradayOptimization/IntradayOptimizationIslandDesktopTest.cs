using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationIslandDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public Func<IGridlockManager> LockManager;

		[Test]
		public void ShouldNotPlaceSameShiftForAllAgentsInIsland()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16))};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(15, TimeSpan.FromMinutes(90)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneDay(dateOnly);
				var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 16));
				asses.Add(ass);
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDay);
			var optimizationPreferences = new OptimizationPreferences{General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true}};
			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences,null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(15, TimeSpan.FromMinutes(65)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneDay(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 16)));
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDay);
			var oneAgent = asses.First().Person;
			asses.Select(x => x.Person).Where(x => x != oneAgent).ForEach(x => schedulerStateHolderFrom.ChoosenAgents.Remove(x));
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new [] {agent}, new[] {ass}, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new []{agent}, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var scheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = scheduleTag, OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new []{agent}, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, new ScheduleTag()) }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var currScheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, currScheduleTag) }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = KeepOriginalScheduleTag.Instance, OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(currScheduleTag);
		}

		[Test]
		public void ShouldNotSetScheduleTagForNonModifiedAssignment()
		{
			var scenario = new Scenario();
			var phoneActivity = new Activity();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSetWithNonSelectableShiftsOnly = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(9, 15, 9, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetWithNonSelectableShiftsOnly, new Contract("_"), skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent, agent2 }, new[] { ass, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute
				.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldNotDuplicateScheduleData()
		{
			var scenario = new Scenario("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var skill = new Skill("_").For(new Activity("_")).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneWeek(dateOnly);
			var meetingPerson = new MeetingPerson(agent, false);
			var meeting = new Meeting(agent, new List<IMeetingPerson> { meetingPerson }, "subject", "location", "description", new Activity("_"), scenario);
			var personMeeting = new PersonMeeting(meeting, meetingPerson, new DateTimePeriod(2010, 1, 1, 2010, 1, 2));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, 
				new IScheduleData[]
				{
					new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9))),
					personMeeting,
					new PreferenceDay(agent, dateOnly, new PreferenceRestriction()) 
				}, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			var scheduledDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduledDay.PersonAbsenceCollection().Length.Should().Be.EqualTo(1);
			scheduledDay.PersonRestrictionCollection().Length.Should().Be.EqualTo(1);
			scheduledDay.PersonMeetingCollection().Length.Should().Be.EqualTo(1);
			scheduledDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().Count().Should().Be.EqualTo(1);
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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence {InContractTime = true}, new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9)));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent}, new IScheduleData[] {ass, personAbsence}, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var meetingPerson = new MeetingPerson(agent, false);
			var period = new DateTimePeriod(2010, 1, 1, 7, 2010, 1, 1, 8);
			var meeting = new Meeting(agent, new List<IMeetingPerson> { meetingPerson }, "subject", "location", "description", phoneActivity, scenario);
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personMeeting }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) {MaxNumberOf = 1});
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agent }, new IScheduleData[] { ass, ass2 }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UseShiftCategoryLimitations = true} };

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToUndo()
		{
			var undoRedoContainer = new UndoRedoContainer();

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
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			schedulerStateHolderFrom.SchedulingResultState.Schedules.SetUndoRedoContainer(undoRedoContainer);
			undoRedoContainer.CreateBatch("for testing to have one batch to undo below. See if we can put this in optimization code later.");
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(63), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
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
				asses.Add(new PersonAssignment(agent, scenario, dateOnly.AddDays(i)).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, shiftPeriod));
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, asses, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			using (CurrentAuthorization.ThreadlyUse(new missingPermissionsOnAgentsOverlappingSchedulePeriod()))
			{
				Assert.DoesNotThrow(
				() => Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null));
			}
		}
		private class missingPermissionsOnAgentsOverlappingSchedulePeriod : FullPermission
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
				var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
				var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
				skillDays.Add(skillDay);
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDays);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);
			});
		}

		[Test]
		public void ShouldNotCrashIfLockExistsForAgentInAnotherIsland()
		{
			const int numberOfIslands = 2;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var asses = new List<IPersonAssignment>();
			var skillDays = new List<ISkillDay>();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
				var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
				skillDays.Add(skillDay);
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
				asses.Add(new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)));
				LockManager().AddLock(agent, dateOnly, LockType.Normal); 
			}
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDays);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);
			});
		}

		[Test]
		public void ShouldKeepSameAssignmentLayersIfChangeWasRolledBack()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(8, 15, 8, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(30)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var assBefore = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17)).WithId();
			assBefore.ShiftLayers.Single().WithId();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new [] { assBefore}, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			var assAfter = schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment();
			assAfter.Id
				.Should().Be.EqualTo(assBefore.Id);
			assAfter.ShiftLayers.Single().Id
				.Should().Be.EqualTo(assBefore.ShiftLayers.Single().Id);
		}


		[Test]
		public void ShouldRollbackIfPeriodValueIsTheSameToAvoidDbUpdates()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var assBefore = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17)).WithId();
			assBefore.ShiftLayers.Single().WithId();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { assBefore }, skillDay);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			var assAfter = schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment();
			assAfter.Id
				.Should().Be.EqualTo(assBefore.Id);
			assAfter.ShiftLayers.Single().Id
				.Should().Be.EqualTo(assBefore.ShiftLayers.Single().Id);
		}


		[TestCase(0, ExpectedResult = 9)]
		[TestCase(0.1, ExpectedResult = 9)]
		[TestCase(1.1, ExpectedResult = 8)]
		public int ShouldConsiderExternalStaff(double bpoResources)
		{
			var date = new DateOnly(2017, 8, 21);
			var activity = new Activity().WithId();
			var skill = new Skill().DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario {DefaultScenario = true};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet8 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(9, 0, 9, 0, 15), shiftCategory));
			var ruleSet10 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(10, 0, 10, 0, 15), shiftCategory));
			var bag = new RuleSetBag(ruleSet8, ruleSet10);
			var agentToOptimize = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(bag, skill).WithSchedulePeriodOneDay(date);
			var agentToOptimizeAss = new PersonAssignment(agentToOptimize, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 9));
			var alreadyScheduledAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(bag, skill).WithSchedulePeriodOneDay(date);
			var alreadyScheduledAgentAss = new PersonAssignment(alreadyScheduledAgent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(0, 24));
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, date, 2, new Tuple<TimePeriod, double>(new TimePeriod(9, 10), 3));
			var externalStaff = new ExternalStaff(bpoResources, new[] { skill }, new DateTimePeriod(new DateTime(date.Date.AddHours(9).Ticks, DateTimeKind.Utc), new DateTime(date.Date.AddHours(10).Ticks, DateTimeKind.Utc)));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, date, new[]{agentToOptimize, alreadyScheduledAgent}, new[]{agentToOptimizeAss, alreadyScheduledAgentAss}, skillDay, externalStaff);
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agentToOptimize }, new DateOnlyPeriod(date, date), optimizationPreferences, null);

			return schedulerStateHolderFrom.Schedules[agentToOptimize].ScheduledDay(date).PersonAssignment().Period.StartDateTime.Hour;
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = true)]
		public bool ShouldWorkWithAndWithoutUseAverageShiftLength(bool useAvaregeShiftLength)
		{
			var scenario = new Scenario();
			var phoneActivity = new Activity { InContractTime = true };
			var lunchActivity = new Activity { InContractTime = false };
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), new ShiftCategory("_").WithId()));
			ruleSet1.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(11, 0, 11, 0, 60)));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSet1, ruleSet2);
			var contract = new ContractWithMaximumTolerance { WorkTime = new WorkTime(new TimeSpan(8, 0, 0)) };
			var skill = new Skill().For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(12, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 18)).WithLayer(lunchActivity, new TimePeriod(12, 13));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, skillDay);

			var optimizationPreferences = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true},
				Shifts = {KeepStartTimes = true, KeepEndTimes = true },
				Advanced = {UseAverageShiftLengths = useAvaregeShiftLength}
			};

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			return schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().ShiftLayers.Any(x => x.Period.StartDateTime.Hour == 11);
		}

		[TestCase(true, ExpectedResult = false)]
		[TestCase(false, ExpectedResult = true)]
		public bool ShouldNotPlaceShiftOutsideOpenHours(bool isClosedOnWeekends)
		{
			var scenario = new Scenario();
			var dateOnly = new DateOnly(2018, 1, 5);
			var phone = new Activity{RequiresSkill = true};
			var other = new Activity("other"){RequiresSkill = true}; 
			var phoneSkill = new Skill().For(phone).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var otherActivity = new Skill().For(other).InTimeZone(TimeZoneInfo.Utc).WithId();
			if (isClosedOnWeekends)
			{
				otherActivity.IsClosedDuringWeekends(); 
			}
			else
			{
				otherActivity.IsOpen();
			}
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phone, new TimePeriodWithSegment(21, 15, 21, 15, 15), new TimePeriodWithSegment(30, 15, 30, 15, 15), new ShiftCategory().WithId()));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(other, new TimePeriodWithSegment(0, 15, 0, 15, 15), new TimePeriodWithSegment(30, 0, 30, 0, 15)));
			var skillDayPhone2 = phoneSkill.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var skillDayPhone3 = phoneSkill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 100);
			var skillDayOther1 = otherActivity.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), 0.1);
			var skillDayOther2 = otherActivity.CreateSkillDayWithDemand(scenario, dateOnly, 0.1);
			var skillDayOther3 = otherActivity.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 0.1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), phoneSkill, otherActivity).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory().WithId()).WithLayer(phone, new TimePeriod(13, 22));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(1)), agent, ass, new[] { skillDayPhone2, skillDayPhone3, skillDayOther1, skillDayOther2, skillDayOther3 });
			var optimizationPreferences = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true } };

			Target.Execute(new NoSchedulingProgress(), schedulerStateHolderFrom, new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, null);

			return schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Equals(15);
		}
	}
}