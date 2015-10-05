using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Scenarios
{
	/*
	Scenario: "expose bug scenario"

given I have scheduled Kalle with
|Date|Time
|2015-09-28|8-17
|2015-09-30|8-17
|2015-10-02|8-17
|2015-10-03|8-17
|2015-10-04|8-17
|2015-10-04|8-17
and I have scheduled Kalle with
|Day Off|Date
|DO|2015-09-29
and Kalle is having a person period between 2015 and 2016
where contract saying to having 40 hours of weekly rest
when I optimize for 2015-09-28 and say shift start time should be kept
then I expect Kalle to still be scheduled on 2015-09-28 and start time should be kept

	*/

	[DomainTest]
	public class WeeklyRestSolverTest
	{
		public IWeeklyRestSolverCommand Target;
		public IMatrixListFactory MatrixListFactory;
		public SchedulerStateHolder SchedulerStateHolder;

		[Test]
		public void ShouldKeepShifttWhenOptimizeEvenIfWeeklyRestIsBrokenAndKeepStartTimeRestrictionIsSet()
		{
			var selectedPeriod = new DateOnlyPeriod(2015, 9, 28, 2015, 10, 4);
			var scenario = new Scenario("unimportant");
			var activity = new Activity("in worktime") { InWorkTime = true, InContractTime = true, RequiresSkill = true };
			var shiftCategory = new ShiftCategory("unimportant");

			shiftCategory.SetId(Guid.NewGuid());
			var kalle = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			var weeklyRest = TimeSpan.FromHours(40);
			kalle.Period(selectedPeriod.StartDate).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(54), TimeSpan.Zero, weeklyRest);
			kalle.Period(selectedPeriod.StartDate).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(17 - 8));
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 20, 0, 60), shiftCategory));

			var ruleSetbag = new RuleSetBag();
			ruleSetbag.AddRuleSet(ruleSet);
			kalle.Period(selectedPeriod.StartDate).RuleSetBag = ruleSetbag;
			kalle.SetId(Guid.NewGuid());

			var skill = SkillFactory.CreateSkill("skill");
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			var personPeriod = (IPersonPeriodModifySkills)kalle.Period(selectedPeriod.StartDate);
			personPeriod.AddPersonSkill(personSkill);

			var schedulePeriod = new SchedulePeriod(selectedPeriod.StartDate, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			kalle.AddSchedulePeriod(schedulePeriod);

			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneInfo.Utc);
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(SchedulerStateHolder.RequestedPeriod.Period());
			SchedulerStateHolder.FilterPersons(new[] { kalle });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(kalle);

			var optimizationPref = new OptimizationPreferences
			{
				Shifts = { KeepStartTimes = true },
				Extra = { TeamGroupPage = GroupPageLight.SingleAgentGroup("blajj") }
			};
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { kalle }).VisiblePeriod);


			foreach (var date in selectedPeriod.DayCollection())
			{
				var ass = new PersonAssignment(kalle, scenario, date);
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
			var matrixlist = MatrixListFactory.CreateMatrixListAll(selectedPeriod);
			matrixlist.First().UnlockPeriod(selectedPeriod);

			SchedulerStateHolder.SchedulingResultState.Skills.Add(skill);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, selectedPeriod.StartDate, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, selectedPeriod);
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IList<ISkillDay>>(skill, new[] { skillDay }));

			Target.Execute(new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPref),
				optimizationPref,
				new[] { kalle },
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

			scheduleDictionary[kalle].ScheduledDay(selectedPeriod.StartDate)
				.PersonAssignment()
				.ShiftLayers.First()
				.Period.StartDateTime
				.Should().Be.EqualTo(new DateTime(selectedPeriod.StartDate.Year, selectedPeriod.StartDate.Month, selectedPeriod.StartDate.Day, 8, 0, 0, DateTimeKind.Utc));
		}
	}
}