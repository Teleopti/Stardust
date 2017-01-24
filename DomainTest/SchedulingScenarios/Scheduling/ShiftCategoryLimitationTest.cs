using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
	public class ShiftCategoryLimitationTest
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldTryToReplaceFirstShiftIfSecondWasUnsuccessful()
		{
			var firstDate = new DateOnly(2017, 1, 22);
			var secondDate = firstDate.AddDays(1);
			var shiftCategoryA = new ShiftCategory("A").WithId();
			var shiftCategoryB = new ShiftCategory("B").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var nightlyRest = TimeSpan.FromHours(1);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), nightlyRest, TimeSpan.FromHours(16)) };
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDayFirstDay = skill.CreateSkillDayWithDemand(scenario, firstDate, 10);
			var skillDaySecondDay = skill.CreateSkillDayWithDemand(scenario, secondDate, 1); //should try to fix this one first
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(14, 0, 14, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategoryB));
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true,
					UseShiftCategoryLimitations = true
				}
			};
			var agent = new Person().WithSchedulePeriodOneWeek(firstDate).WithPersonPeriod(ruleSet, contract, skill);
			agent.SchedulePeriod(firstDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategoryA)
			{
				MaxNumberOf = 1
			});
			var period = new DateOnlyPeriod(firstDate, secondDate);
			var assA = new PersonAssignment(agent, scenario, firstDate).ShiftCategory(shiftCategoryA).WithLayer(activity, new TimePeriod(6, 14));
			var assB = new PersonAssignment(agent, scenario, secondDate).ShiftCategory(shiftCategoryA).WithLayer(activity, new TimePeriod(6, 14));
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new[] { assA, assB }, new[] { skillDayFirstDay, skillDaySecondDay });
			stateholder.SchedulingResultState.UseValidation = true;	
			var scheduleDays = stateholder.Schedules[agent].ScheduledDayCollection(period).ToList();

			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), scheduleDays, new OptimizationPreferences(), null);

			stateholder.Schedules[agent].ScheduledDay(firstDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryA);
			stateholder.Schedules[agent].ScheduledDay(secondDate).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryB);
		}
	}
}