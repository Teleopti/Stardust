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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationIslandDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizeIntradayIslandsDesktop Target;
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

			Target.Optimize(schedulerStateHolderFrom.Schedules.SchedulesForDay(dateOnly).Select(x => x.Person).Distinct(), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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
			asses.Select(x => x.Person).Where(x => x != oneAgent).ForEach(x => schedulerStateHolderFrom.AllPermittedPersons.Remove(x));

			Target.Optimize(new[] { oneAgent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());
			
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
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = scheduleTag;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

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
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = NullScheduleTag.Instance;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, 
				new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, new ScheduleTag()) }, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

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
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = KeepOriginalScheduleTag.Instance;
			var currScheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent },
				new IPersistableScheduleData[] { ass, new AgentDayScheduleTag(agent, dateOnly, scenario, currScheduleTag) }, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

			schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).ScheduleTag().Should().Be.SameInstanceAs(currScheduleTag);
		}

		[Test]
		public void ShouldNotSetScheduleTagForNonModifiedAssignment()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(9, 15, 9, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = new ScheduleTag();
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

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
			schedulerStateHolderFrom.Schedules.TakeSnapshot();

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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
		
			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence {InContractTime = true}, new DateTimePeriod(2010, 1, 1, 8, 2010, 1, 1, 9)));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent}, new IScheduleData[] {ass, personAbsence}, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new RotationRestriction {ShiftCategory = shiftCategory};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseRotations = true;
			optimizationPreferences.General.RotationsValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new AvailabilityRestriction {NotAvailable = true};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseAvailabilities = true;
			optimizationPreferences.General.AvailabilitiesValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

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
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UsePreferences = true;
			optimizationPreferences.General.PreferencesValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

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
			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());
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

			Target.Optimize(new[] {agent}, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());

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
			SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);

			using (CurrentAuthorization.ThreadlyUse(new missingPermissionsOnAgentsOverlappingSchedulePeriod()))
			{
				Assert.DoesNotThrow(
				() =>
					Target.Optimize(new[] { agent},
						new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback()));
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
			SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDays);

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());
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
			SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, skillDays);

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), new NoIntradayOptimizationCallback());
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
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = KeepOriginalScheduleTag.Instance;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new [] { assBefore}, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

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
			var optPref = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optPref.General.ScheduleTag = KeepOriginalScheduleTag.Instance;
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { assBefore }, skillDay);

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optPref, new NoIntradayOptimizationCallback());

			var assAfter = schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment();
			assAfter.Id
				.Should().Be.EqualTo(assBefore.Id);
			assAfter.ShiftLayers.Single().Id
				.Should().Be.EqualTo(assBefore.ShiftLayers.Single().Id);
		}

		public IntradayOptimizationIslandDesktopTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, bool resourcePlannerSpeedUpShiftsWithinDay45694) : base(resourcePlannerMergeTeamblockClassicIntraday45508, resourcePlannerSpeedUpShiftsWithinDay45694)
		{
		}
	}
}