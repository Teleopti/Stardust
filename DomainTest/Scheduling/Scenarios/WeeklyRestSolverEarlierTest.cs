using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Scenarios
{
	/* Obsolute tests! Delete next time they are "ivägen"
 	 * When writing weeklyrest test, write tests against
	 * something that real code execute, eg scheduling.
	 * Don't write tests against inner service "IWeeklyRestSolverCommand"
 	*/
	[DomainTest]
	public class WeeklyRestSolverEarlierTest
	{
		public WeeklyRestSolverCommand Target;
		public MatrixListFactory MatrixListFactory;
		public SchedulerStateHolder SchedulerStateHolder;
		public IResourceCalculation CascadingResourceCalculation;
		public CascadingResourceCalculationContextFactory CascadingResourceCalculationContextFactory;
		public ITimeZoneGuard TimeZoneGuard;

		private DateOnlyPeriod weekPeriod = new DateOnlyPeriod(2015, 9, 28, 2015, 10, 04);
		private readonly IScenario scenario = new Scenario("unimportant");

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndAlterBetweenRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { AlterBetween = true, SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(12))},
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.EndDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 17, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepTimeOfActivityRestrictionIsSet()
		{
			var extendedActivity = new Activity("extendedActivity") { InWorkTime = true, InContractTime = true, RequiresSkill = false }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var extender = new ActivityAbsoluteStartExtender(extendedActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(6, 0, 6, 0, 15));
			ruleSet.AddExtender(extender);
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var personAssignment = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment();
			var mainActivity = personAssignment.MainActivities().First();

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = mainActivity.Payload},
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Period
				.Should().Be.EqualTo(mainActivity.Period);
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndActivityIsNotSameAndKeepActivityRestrictionIsNotEmpty()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activityForRuleSet);
			setUpSchedules(agent, activity, shiftCategory);

			var personAssignment = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment();
			var mainActivity = personAssignment.MainActivities().First();
			var selectedActivities = new List<IActivity> { mainActivity.Payload };

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { SelectedActivities = selectedActivities },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Payload
				.Should().Be.EqualTo(mainActivity.Payload);
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndActivityPeriodIsNotSameAndKeepActivitiesRestrictionIsNotEmpty()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var personAssignment = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment();
			var mainActivity = personAssignment.MainActivities().First();
			var selectedActivities = new List<IActivity> { mainActivity.Payload };

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { SelectedActivities = selectedActivities},
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Period
				.Should().Be.EqualTo(mainActivity.Period);
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepShiftCategoryRestrictionIsSet()
		{
			var shiftCategoryForRuleSet = new ShiftCategory("shiftCategoryForRuleSet").WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategoryForRuleSet));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var shiftCategoryScheduled = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment().ShiftCategory;

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepShiftCategories = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftCategory
				.Should().Be.EqualTo(shiftCategoryScheduled);
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepEndTimeRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();
			
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);


			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepEndTimes = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new [] {agent}, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.EndDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 17, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepStartTimeRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity,shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepStartTimes = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldNotKeepShiftWhenOptimizeAndWeeklyRestIsBrokenAndDontMoveActivityRestrictionNotAffectShift()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activityForRuleSet);
			setUpSchedules(agent, activity, shiftCategory);

			var selectedActivities = new List<IActivity> { activityForRuleSet };

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { SelectedActivities = selectedActivities },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotKeepShiftWhenOptimizeIfWeeklyRestIsBrokenAndAlterBetweenRestrictionNotAffectShift()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { AlterBetween = true, SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36)) },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotKeepShiftWhenOptimizeIfWeeklyRestIsBrokenAndKeepActivityLengthRestrictionNotAffectShift()
		{

			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = activityForRuleSet },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(new[] { agent }, optimizationPref, dayOffOptimzationPreferenceProvider);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteDayAfterDayOffIfDayBeforeDayOffIsLocked()
		{

			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true }.WithId();
			var shiftCategory = new ShiftCategory("unimportant").WithId();

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));

			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = activityForRuleSet },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);

			executeTarget(SchedulerStateHolder.Schedules, new[] {agent}, optimizationPref, dayOffOptimzationPreferenceProvider,
				new DateOnlyPeriod(weekPeriod.StartDate.AddDays(2), weekPeriod.StartDate.AddDays(2)));

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate.AddDays(2))
				.PersonAssignment()
				.ShiftLayers.Should().Be.Empty();
		}

		private void executeTarget(IScheduleDictionary schedules, IList<IPerson> agents, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, DateOnlyPeriod selectedPeriod)
		{
			var matrixlist = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, agents, selectedPeriod);

			using(CascadingResourceCalculationContextFactory.Create(SchedulerStateHolder.SchedulingResultState, false, selectedPeriod))
			{
				Target.Execute(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences),
					optimizationPreferences,
					agents,
					new SchedulePartModifyAndRollbackService(
						SchedulerStateHolder.SchedulingResultState,
						new SchedulerStateScheduleDayChangedCallback(
							new ScheduleChangesAffectedDates(TimeZoneGuard), 
							() => SchedulerStateHolder
						),
						new ScheduleTagSetter(
							new NullScheduleTag()
						)
					),
					new ResourceCalculateDelayer(CascadingResourceCalculation, true, SchedulerStateHolder.SchedulingResultState, UserTimeZone.Make()),
					selectedPeriod,
					matrixlist,
					new NoSchedulingProgress(),
					dayOffOptimizationPreferenceProvider
				);
			}
		}

		private void executeTarget(IList<IPerson> agents, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedPeriod = new DateOnlyPeriod(weekPeriod.StartDate, weekPeriod.StartDate);
			executeTarget(SchedulerStateHolder.Schedules, agents, optimizationPreferences, dayOffOptimizationPreferenceProvider, selectedPeriod);
		}

		private void setUpSchedules(IPerson agent, IActivity activity, IShiftCategory shiftCategory)
		{
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(weekPeriod, TimeZoneInfo.Utc);
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			foreach (var date in weekPeriod.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				if (date == new DateOnly(2015, 09, 29))
				{
					ass.SetDayOff(new DayOffTemplate());
				}
				else
				{
					var eightOClock = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0, DateTimeKind.Utc);
					var seventteenOClock = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0, DateTimeKind.Utc);
					ass.AddActivity(activity, new DateTimePeriod(eightOClock, seventteenOClock));
					ass.SetShiftCategory(shiftCategory);
				}
				scheduleDictionary.AddPersonAssignment(ass);
			}

			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;	
		}

		private IPerson setupAgent(IWorkShiftRuleSet ruleSet, IActivity skillActivity)
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue).WithId();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var weeklyRest = TimeSpan.FromHours(40);
			var personPeriod = agent.Period(weekPeriod.StartDate);
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, weeklyRest);
			personPeriod.PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(17 - 8));
			
			personPeriod.RuleSetBag = new RuleSetBag(ruleSet);

			var skill = SkillFactory.CreateSkill("skill");
			skill.Activity = skillActivity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			agent.AddSkill(personSkill,personPeriod);

			var schedulePeriod = new SchedulePeriod(weekPeriod.StartDate, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			agent.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.SchedulingResultState.AddSkills(skill);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, weekPeriod.StartDate, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, weekPeriod);
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IEnumerable<ISkillDay>>(skill, new[] { skillDay }));

			return agent;
		}

		[Test]
		public void ShouldHandleAgentsInUtcPlus8Bug37665()
		{
			var phoneActivity = ActivityFactory.CreateActivity("_");
			phoneActivity.InWorkTime = true;
			phoneActivity.InContractTime = true;
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 21);
			weekPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7));
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(0, 0, 23, 0, 15),
					new TimePeriodWithSegment(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(31)), TimeSpan.FromMinutes(15)),
					shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(42))
			};
			var skill =
				new Skill("_", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfoFactory.MoskowTimeZoneInfo()
				}.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			SchedulerStateHolder.SchedulingResultState.AddSkills(skill);
			var skillDayMo = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillDayTu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60));
			var skillDayWe = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(2), TimeSpan.FromMinutes(60));
			var skillDayTh = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(3), TimeSpan.FromMinutes(60));
			var skillDayFr = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(4), TimeSpan.FromMinutes(60));
			var skillDaySa = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(5), TimeSpan.FromMinutes(60));
			var skillDaySu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(6), TimeSpan.FromMinutes(60));
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IEnumerable<ISkillDay>>(skill,
				new[] { skillDayMo, skillDayTu, skillDayWe, skillDayTh, skillDayFr, skillDaySa, skillDaySu }));

			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new[] { skill }).WithId();
			agent.Period(dateOnly).PersonContract.Contract = contract;
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.RussiaTz7ZoneInfo());
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(weekPeriod, TimeZoneInfoFactory.MoskowTimeZoneInfo());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			foreach (var date in weekPeriod.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				if (date == new DateOnly(2016, 03, 23))
				{
					ass.SetDayOff(new DayOffTemplate(new Description("DayOff")));
				}
				else
				{
					var startTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(7).AddMinutes(45), agent.PermissionInformation.DefaultTimeZone());
					var endTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(15).AddMinutes(45), agent.PermissionInformation.DefaultTimeZone());
					ass.AddActivity(phoneActivity, new DateTimePeriod(startTimeUtc, endTimeUtc));
					ass.SetShiftCategory(shiftCategory);
				}
				scheduleDictionary.AddPersonAssignment(ass);
			}

			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var selectedPeriod = new DateOnlyPeriod(2016, 3, 22, 2016, 3, 22);
			var matrixlist = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, SchedulerStateHolder.SchedulingResultState.LoadedAgents, selectedPeriod);
			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);
			var optimizationPreferences = new OptimizationPreferences();
			using(CascadingResourceCalculationContextFactory.Create(SchedulerStateHolder.SchedulingResultState, false, selectedPeriod))
			{
				Target.Execute(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences),
					optimizationPreferences,
					new List<IPerson> { agent },
					new SchedulePartModifyAndRollbackService(
						SchedulerStateHolder.SchedulingResultState,
						new SchedulerStateScheduleDayChangedCallback(
							new ScheduleChangesAffectedDates(TimeZoneGuard),
							() => SchedulerStateHolder
						),
						new ScheduleTagSetter(
							new NullScheduleTag()
						)
					),
					new ResourceCalculateDelayer(CascadingResourceCalculation, true, SchedulerStateHolder.SchedulingResultState, UserTimeZone.Make()),
					selectedPeriod,
					matrixlist,
					new NoSchedulingProgress(),
					dayOffOptimzationPreferenceProvider
				);
			}

			var movedSchedule = scheduleDictionary[agent].ScheduledDay(new DateOnly(2016, 3, 22));
			var newEndTime = movedSchedule.PersonAssignment().Period.EndDateTimeLocal(movedSchedule.TimeZone);

			newEndTime.TimeOfDay.TotalHours.Should().Be.LessThan(13.8); //13:45 is maximum end time
		}

		[Test]
		public void ShouldNotThrowIfCalledAfterSchedulingAndOptimizationPreferencesIsNullBug43798()
		{
			var phoneActivity = ActivityFactory.CreateActivity("_");
			phoneActivity.InWorkTime = true;
			phoneActivity.InContractTime = true;
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2016, 3, 21);
			weekPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7));
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(0, 0, 23, 0, 15),
					new TimePeriodWithSegment(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(31)), TimeSpan.FromMinutes(15)),
					shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(42))
			};
			var skill =
				new Skill("_", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfoFactory.MoskowTimeZoneInfo()
				}.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			SchedulerStateHolder.SchedulingResultState.AddSkills(skill);
			var skillDayMo = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillDayTu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60));
			var skillDayWe = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(2), TimeSpan.FromMinutes(60));
			var skillDayTh = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(3), TimeSpan.FromMinutes(60));
			var skillDayFr = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(4), TimeSpan.FromMinutes(60));
			var skillDaySa = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(5), TimeSpan.FromMinutes(60));
			var skillDaySu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(6), TimeSpan.FromMinutes(60));
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IEnumerable<ISkillDay>>(skill,
				new[] { skillDayMo, skillDayTu, skillDayWe, skillDayTh, skillDayFr, skillDaySa, skillDaySu }));

			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new[] { skill }).WithId();
			agent.Period(dateOnly).PersonContract.Contract = contract;
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.RussiaTz7ZoneInfo());
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(weekPeriod, TimeZoneInfoFactory.MoskowTimeZoneInfo());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.LoadedAgents.Add(agent);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent }).VisiblePeriod);
			foreach (var date in weekPeriod.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				if (date == new DateOnly(2016, 03, 23))
				{
					ass.SetDayOff(new DayOffTemplate(new Description("DayOff")));
				}
				else
				{
					var startTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(7).AddMinutes(45), agent.PermissionInformation.DefaultTimeZone());
					var endTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(15).AddMinutes(45), agent.PermissionInformation.DefaultTimeZone());
					ass.AddActivity(phoneActivity, new DateTimePeriod(startTimeUtc, endTimeUtc));
					ass.SetShiftCategory(shiftCategory);
				}
				scheduleDictionary.AddPersonAssignment(ass);
			}

			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			var selectedPeriod = new DateOnlyPeriod(2016, 3, 22, 2016, 3, 22);
			var matrixlist = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, SchedulerStateHolder.SchedulingResultState.LoadedAgents, selectedPeriod);
			var dayOffPreferences = new DaysOffPreferences();
			var dayOffOptimzationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffPreferences);
			var optimizationPreferences = new OptimizationPreferences();
			using (CascadingResourceCalculationContextFactory.Create(SchedulerStateHolder.SchedulingResultState, false, selectedPeriod))
			{
				Target.Execute(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences),
					null,
					new List<IPerson> { agent },
					new SchedulePartModifyAndRollbackService(
						SchedulerStateHolder.SchedulingResultState,
						new SchedulerStateScheduleDayChangedCallback(
							new ScheduleChangesAffectedDates(TimeZoneGuard),
							() => SchedulerStateHolder
						),
						new ScheduleTagSetter(
							new NullScheduleTag()
						)
					),
					new ResourceCalculateDelayer(CascadingResourceCalculation, true, SchedulerStateHolder.SchedulingResultState, UserTimeZone.Make()),
					selectedPeriod,
					matrixlist,
					new NoSchedulingProgress(),
					dayOffOptimzationPreferenceProvider
				);
	
			}
			var movedSchedule = scheduleDictionary[agent].ScheduledDay(new DateOnly(2016, 3, 22));
			var newEndTime = movedSchedule.PersonAssignment().Period.EndDateTimeLocal(movedSchedule.TimeZone);

			newEndTime.TimeOfDay.TotalHours.Should().Be.LessThan(13.8); //13:45 is maximum end time
		}
	}
}