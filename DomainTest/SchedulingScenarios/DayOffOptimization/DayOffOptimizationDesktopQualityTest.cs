using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Calculation;
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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	public class DayOffOptimizationDesktopQualityTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktopTeamBlock Target;
		public IScheduleResultDataExtractorProvider ScheduleResultDataExtractorProvider;
		public IUserTimeZone UserTimeZone;

		[Test, Ignore("Until investigated")]
		public void ShouldProduceGoodStandardDevBetweenDaysWhenUsingSingleAgentSingleDayAndNoDoSettings()
		{
			var firstDay = new DateOnly(2017, 09, 04); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 4);
			var activity = new Activity("_");
			var skill = new Skill().For(activity).InTimeZone(UserTimeZone.TimeZone()).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agentList = new List<IPerson>();
			var assesList = new List<IPersonAssignment>();
			for (var n = 0; n < 30; n++)
			{
				var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill).InTimeZone(UserTimeZone.TimeZone());
				agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 4));
				agent.SchedulePeriod(firstDay).SetDaysOff(8);
				var asses = Enumerable.Range(0, 28).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
				asses[5].SetDayOff(new DayOffTemplate()); //saturday
				asses[6].SetDayOff(new DayOffTemplate());
				asses[12].SetDayOff(new DayOffTemplate());
				asses[13].SetDayOff(new DayOffTemplate());
				asses[19].SetDayOff(new DayOffTemplate());
				asses[20].SetDayOff(new DayOffTemplate());
				asses[26].SetDayOff(new DayOffTemplate());
				asses[27].SetDayOff(new DayOffTemplate());
				agentList.Add(agent);
				assesList.AddRange(asses);
			}

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agentList, assesList, skillDays);
			
			Target.Execute(period, agentList, new NoSchedulingProgress(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } },
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences
				{
					UseConsecutiveDaysOff = false,
					UseDaysOffPerWeek = false,
					UseConsecutiveWorkdays = false,
					UseFullWeekendsOff = false,
				}), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var allSkillDataExctractor = ScheduleResultDataExtractorProvider.CreateAllSkillsDataExtractor(period, stateHolder.SchedulingResultState, new AdvancedPreferences());
			var standardDeviation = Variances.StandardDeviation(allSkillDataExctractor.Values().Select(v => v.GetValueOrDefault(-1)));
			standardDeviation.Should().Be.LessThan(0.034d);
		}

		[Test, Ignore("Until investigated")]
		public void ShouldProduceGoodStandardDevBetweenDaysWhenUsingSingleAgentSingleDayAndFreeWeekEndSettings()
		{
			var firstDay = new DateOnly(2017, 09, 04); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 4);
			var activity = new Activity("_");
			var skill = new Skill().For(activity).InTimeZone(UserTimeZone.TimeZone()).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agentList = new List<IPerson>();
			var assesList = new List<IPersonAssignment>();
			for (var n = 0; n < 30; n++)
			{
				var agent = new Person().WithId().InTimeZone(UserTimeZone.TimeZone()).WithPersonPeriod(ruleSet, skill);
				agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 4));
				agent.SchedulePeriod(firstDay).SetDaysOff(8);
				var asses = Enumerable.Range(0, 28).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
				asses[5].SetDayOff(new DayOffTemplate()); //saturday
				asses[6].SetDayOff(new DayOffTemplate());
				asses[12].SetDayOff(new DayOffTemplate());
				asses[13].SetDayOff(new DayOffTemplate());
				asses[19].SetDayOff(new DayOffTemplate());
				asses[20].SetDayOff(new DayOffTemplate());
				asses[26].SetDayOff(new DayOffTemplate());
				asses[27].SetDayOff(new DayOffTemplate());
				agentList.Add(agent);
				assesList.AddRange(asses);
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agentList, assesList, skillDays);
			
			Target.Execute(period, agentList, new NoSchedulingProgress(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } },
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences
				{
					UseConsecutiveDaysOff = true,
					ConsecutiveDaysOffValue = new MinMax<int>(1, 3),
					UseDaysOffPerWeek = true,
					DaysOffPerWeekValue = new MinMax<int>(2, 2),
					UseConsecutiveWorkdays = true,
					ConsecutiveWorkdaysValue = new MinMax<int>(1, 6),
					UseFullWeekendsOff = true,
					FullWeekendsOffValue = new MinMax<int>(1, 2)
				}), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var allSkillDataExctractor = ScheduleResultDataExtractorProvider.CreateAllSkillsDataExtractor(period, stateHolder.SchedulingResultState, new AdvancedPreferences());
			var standardDeviation = Variances.StandardDeviation(allSkillDataExctractor.Values().Select(v => v.GetValueOrDefault(-1)));
			standardDeviation.Should().Be.LessThan(0.034d);
		}
	}
}