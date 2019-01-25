using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingDesktopBlockTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		
		[TestCase(true, false)]
		[TestCase(false, true)]
		public void ShouldHandleBlockBetweenDaysOffWhenMonthEndsOnTuesday(bool sameShiftCategory, bool sameShift)
		{
			var period = new DateOnlyPeriod(2017, 10, 1, 2017, 11, 4);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen(new TimePeriod(8, 20));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 12, 0, 60),
				new TimePeriodWithSegment(16, 0, 20, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromMinutes(60)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, new ContractScheduleWorkingMondayToFriday(), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, Enumerable.Empty<IScheduleData>(), skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameShiftCategory = sameShiftCategory,
				BlockSameShift = sameShift,
				UseBlock = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, period);

			stateHolder.Schedules[agent].CalculatedCurrentScheduleSummary(new DateOnlyPeriod(2017, 10, 1, 2017, 10, 31))
				.ContractTime.Should().Be.EqualTo(TimeSpan.FromHours(176));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldRespectBlockSameShiftCategoryInBetweenPersonPeriods(bool hasExtraPersonPeriod)
		{
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCatExpected = new ShiftCategory("expected").WithId();
			var shiftCatNotExpected = new ShiftCategory("not expected").WithId();
			var scenario = new Scenario();
			var activity = new Activity();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDayWithDemandOnInterval(scenario, period, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), 0));
			var ruleSetExpected = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCatExpected));
			var ruleSetNotExpected = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCatNotExpected));
			var ruleSetBag = new RuleSetBag(ruleSetExpected, ruleSetNotExpected);
			var agent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId()
					.WithPersonPeriod(ruleSetBag, skill);
			if(hasExtraPersonPeriod)
					agent.WithPersonPeriod(date.AddDays(2), ruleSetBag, skill);
			var stateholder = SchedulerStateHolder.Fill(scenario, period, agent, 
					new []
					{
						new PersonAssignment(agent, scenario, date).WithDayOff(),
						new PersonAssignment(agent, scenario, date.AddDays(1)).WithLayer(activity, new TimePeriod(8, 17)).ShiftCategory(shiftCatExpected),
						new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff(),
					},
					skillDays
			);
			var schedulingOptions = new SchedulingOptions
			{ 
					UseBlock = true,
					BlockSameShiftCategory = true,
					BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			};
	
			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[]{agent}, new DateOnlyPeriod(date.AddDays(2), date.AddDays(5)));

			foreach (var day in new DateOnlyPeriod(date.AddDays(1), date.AddDays(5)).DayCollection())
			{
				stateholder.Schedules[agent].ScheduledDay(day).PersonAssignment().ShiftCategory
					.Should().Be.EqualTo(shiftCatExpected);
			}
		}

		[TestCase(0, true)]
		[TestCase(2, false)]
		public void ShouldNotBeScheduledBeforeHiredWhenHiredInTheMiddleOfTheBlockPeriod(int personPeriodStartIntoBlockPeriod, bool daysScheduled)
		{
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var scenario = new Scenario();
			var activity = new Activity();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId()
				.WithPersonPeriod(date.AddDays(personPeriodStartIntoBlockPeriod), ruleSet, skill);
			var stateholder = SchedulerStateHolder.Fill(scenario, period, agent,new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff(), skillDays);
			var schedulingOptions = new SchedulingOptions
			{ 
				UseBlock = true,
				BlockSameShiftCategory = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff
			};
			
			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[]{agent}, period);

			stateholder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.EqualTo(daysScheduled);
			stateholder.Schedules[agent].ScheduledDay(date.AddDays(1)).IsScheduled().Should().Be.EqualTo(daysScheduled);
		}

		public SchedulingDesktopBlockTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}