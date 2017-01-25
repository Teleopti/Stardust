using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_ShiftCategoryLimitations_42680)]
	public class ShiftCategoryLimitationTest
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldTryToReplaceSecondShiftIfFirstWasUnsuccessful()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightRest = TimeSpan.FromHours(11);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 1); //should try with this one first
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 10); 
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true,
					UseShiftCategoryLimitations = true
				}
			};
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });
			var scheduleDays = stateholder.Schedules[agent].ScheduledDayCollection(period).ToList();

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), scheduleDays, new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryBefore);
			stateholder.Schedules[agent].ScheduledDay(secondDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAfter);
		}

		[Test, Ignore("to be fixed")]
		public void ShouldRemoveShiftToFulfillLimitation()
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCategory = new ShiftCategory("_").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(11), TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory));
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true,
					UseShiftCategoryLimitations = true
				}
			};
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(date, date);
			var assA = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, date.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB}, new[] { skillDay});
			var scheduleDays = stateholder.Schedules[agent].ScheduledDayCollection(period).ToList();

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), scheduleDays, new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(date).PersonAssignment(true).ShiftLayers.Should().Be.Empty();
			stateholder.Schedules[agent].ScheduledDay(date.AddDays(1)).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategory);
		}


		[Test, Ignore("To be fixed. Part of PBI 42680")]
		public void ShouldReplaceBestDayFirst()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryBefore = new ShiftCategory("Before").WithId();
			var shiftCategoryAfter = new ShiftCategory("After").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 10); 
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 1); //should try with this one first
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryAfter));
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true,
					UseShiftCategoryLimitations = true
				}
			};
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryBefore) { MaxNumberOf = 1 });
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryBefore).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });
			var scheduleDays = stateholder.Schedules[agent].ScheduledDayCollection(period).ToList();

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), scheduleDays, new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryBefore);
			stateholder.Schedules[agent].ScheduledDay(secondDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAfter);
		}
	}
}