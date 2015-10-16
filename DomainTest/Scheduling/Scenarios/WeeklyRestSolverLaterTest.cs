using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Scenarios
{
	[DomainTest, Toggle(Toggles.ResourcePlanner_WeeklyRestSolver_35043), Ignore("Hangs forever now when user has permission to change schedule. ShiftNudgeManager row 126.")]
	public class WeeklyRestSolverLaterTest
	{
		public IWeeklyRestSolverCommand Target;
		public IMatrixListFactory MatrixListFactory;
		public SchedulerStateHolder SchedulerStateHolder;

		private DateOnlyPeriod weekPeriod = new DateOnlyPeriod(2015, 9, 28, 2015, 10, 04);
		private readonly IScenario scenario = new Scenario("unimportant");

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndAlterBetweenRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { AlterBetween = true, SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(12)) },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.EndDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 17, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepTimeOfActivityRestrictionIsSet()
		{
			var extendedActivity = new Activity("extendedActivity") { InWorkTime = true, InContractTime = true, RequiresSkill = false };
			extendedActivity.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var extender = new ActivityAbsoluteStartExtender(extendedActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(6, 0, 6, 0, 15));
			ruleSet.AddExtender(extender);
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var personAssignment = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment();
			var mainActivity = personAssignment.MainActivities().First();

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = mainActivity.Payload },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Period
				.Should().Be.EqualTo(mainActivity.Period);
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndActivityIsNotSameAndKeepActivityRestrictionIsNotEmpty()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activityForRuleSet.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

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

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Payload
				.Should().Be.EqualTo(mainActivity.Payload);
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndActivityPeriodIsNotSameAndKeepActivitiesRestrictionIsNotEmpty()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var personAssignment = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment();
			var mainActivity = personAssignment.MainActivities().First();
			var selectedActivities = new List<IActivity> { mainActivity.Payload };

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { SelectedActivities = selectedActivities },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.MainActivities().First()
				.Period
				.Should().Be.EqualTo(mainActivity.Period);
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepShiftCategoryRestrictionIsSet()
		{
			var shiftCategoryForRuleSet = new ShiftCategory("shiftCategoryForRuleSet");
			shiftCategoryForRuleSet.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategoryForRuleSet));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var shiftCategoryScheduled = SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate).PersonAssignment().ShiftCategory;

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepShiftCategories = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftCategory
				.Should().Be.EqualTo(shiftCategoryScheduled);
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepEndTimeRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);


			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepEndTimes = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.EndDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 17, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepStartTimeRestrictionIsSet()
		{
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepStartTimes = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeAndWeeklyRestIsBrokenAndKeepActivityRestrictionNotAffectShift()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activityForRuleSet.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activityForRuleSet);
			setUpSchedules(agent, activity, shiftCategory);

			var selectedActivities = new List<IActivity> { activityForRuleSet };

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { SelectedActivities = selectedActivities },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeIfWeeklyRestIsBrokenAndAlterBetweenRestrictionNotAffectShift()
		{
			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activityForRuleSet.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));
			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { AlterBetween = true, SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36)) },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldKeepShiftWhenOptimizeIfWeeklyRestIsBrokenAndKeepActivityLengthRestrictionNotAffectShift()
		{

			var activityForRuleSet = new Activity("activityForRuleSet") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activityForRuleSet.SetId(Guid.NewGuid());

			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			activity.SetId(Guid.NewGuid());

			var shiftCategory = new ShiftCategory("unimportant");
			shiftCategory.SetId(Guid.NewGuid());

			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityForRuleSet, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));

			var agent = setupAgent(ruleSet, activity);
			setUpSchedules(agent, activity, shiftCategory);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = activityForRuleSet },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};

			executeTarget(new[] { agent }, optimizationPref);

			SchedulerStateHolder.Schedules[agent].ScheduledDay(weekPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(weekPeriod.StartDate.Year, weekPeriod.StartDate.Month, weekPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}

		private void executeTarget(IList<IPerson> agents, OptimizationPreferences optimizationPreferences)
		{
			var date = new DateOnly(2015, 09, 30);
			var selectedPeriod = new DateOnlyPeriod(date, date);
			var matrixlist = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(weekPeriod);
			matrixlist.First().UnlockPeriod(selectedPeriod);

			Target.Execute(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences),
				optimizationPreferences,
				agents,
				new SchedulePartModifyAndRollbackService(
					SchedulerStateHolder.SchedulingResultState,
					new SchedulerStateScheduleDayChangedCallback(
						new ResourceCalculateDaysDecider(),
						() => SchedulerStateHolder
						),
					new ScheduleTagSetter(
						new NullScheduleTag()
						)
					),
				new ResourceCalculateDelayer(
					new ResourceOptimizationHelper(
						() => SchedulerStateHolder,
						new OccupiedSeatCalculator(),
						new NonBlendSkillCalculator(),
						() => new PersonSkillProvider(),
						new PeriodDistributionService(),
						new IntraIntervalFinderService(
							new SkillDayIntraIntervalFinder(
								new IntraIntervalFinder(),
								new SkillActivityCountCollector(
									new SkillActivityCounter()
									),
								new FullIntervalFinder()
								)
							)
						),
					1,
					true,
					true
					),
				selectedPeriod,
				matrixlist,
				new NoBackgroundWorker()
				);
		}

		private void setUpSchedules(IPerson agent, IActivity activity, IShiftCategory shiftCategory)
		{
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(weekPeriod, TimeZoneInfo.Utc);
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(SchedulerStateHolder.RequestedPeriod.Period());
			SchedulerStateHolder.FilterPersons(new[] { agent });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);

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
			var agent = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var weeklyRest = TimeSpan.FromHours(40);
			agent.Period(weekPeriod.StartDate).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, weeklyRest);
			agent.Period(weekPeriod.StartDate).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(17 - 8));

			var ruleSetbag = new RuleSetBag();
			ruleSetbag.AddRuleSet(ruleSet);
			agent.Period(weekPeriod.StartDate).RuleSetBag = ruleSetbag;
			agent.SetId(Guid.NewGuid());

			var skill = SkillFactory.CreateSkill("skill");
			skill.Activity = skillActivity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			var personPeriod = (IPersonPeriodModifySkills)agent.Period(weekPeriod.StartDate);
			personPeriod.AddPersonSkill(personSkill);

			var schedulePeriod = new SchedulePeriod(weekPeriod.StartDate, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			agent.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.SchedulingResultState.Skills.Add(skill);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, weekPeriod.StartDate, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, weekPeriod);
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IList<ISkillDay>>(skill, new[] { skillDay }));

			return agent;
		}
	}
}
