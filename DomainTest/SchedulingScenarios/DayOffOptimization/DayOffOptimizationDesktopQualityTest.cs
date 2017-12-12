﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	public class DayOffOptimizationDesktopQualityTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;
		public IScheduleResultDataExtractorProvider ScheduleResultDataExtractorProvider;
		public IUserTimeZone UserTimeZone;

		[Test]
		public void ShouldProduceGoodStandardDevBetweenDaysWhenUsingSingleAgentSingleDayAndNoDoSettings()
		{
			var targetStandardDeviation = 0.032;
			var targetAchieved = false;
			for (int i = 0; i < 10; i++)
			{
				var stateHolder = setupStandardState(out var period, out var agentList);
				Target.Execute(period, agentList, new NoSchedulingProgress(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } },
					new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences
					{
						UseConsecutiveDaysOff = false,
						UseDaysOffPerWeek = false,
						UseConsecutiveWorkdays = false,
						UseFullWeekendsOff = false,
					}), (o, args) => { });

				var standardDeviation = getResultingStandardDeviation(period, stateHolder);
				if (standardDeviation <= targetStandardDeviation)
				{
					targetAchieved = true;
					break;
				}
			}
			
			targetAchieved.Should().Be.True();
		}

		[Test]
		public void ShouldProduceGoodStandardDevBetweenDaysWhenUsingSingleAgentSingleDayAndFreeWeekEndSettings()
		{
			var targetStandardDeviation = 0.032;
			var targetAchieved = false;
			for (int i = 0; i < 10; i++)
			{
				var stateHolder = setupStandardState(out var period, out var agentList);
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
					}), (o, args) => { });

				var standardDeviation = getResultingStandardDeviation(period, stateHolder);
				if (standardDeviation <= targetStandardDeviation)
				{
					targetAchieved = true;
					break;
				}
			}

			targetAchieved.Should().Be.True();
		}

		private ISchedulerStateHolder setupStandardState(out DateOnlyPeriod period, out List<IPerson> agentList)
		{
			var firstDay = new DateOnly(2017, 09, 04); //mon
			period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 4);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).InTimeZone(UserTimeZone.TimeZone()).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
				new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agentList = new List<IPerson>();
			var assesList = new List<IPersonAssignment>();
			var dayOffTemplate = new DayOffTemplate();
			for (var n = 0; n < 30; n++)
			{
				var agent = new Person().WithId().InTimeZone(UserTimeZone.TimeZone()).WithPersonPeriod(ruleSet, skill);
				agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 4));
				agent.SchedulePeriod(firstDay).SetDaysOff(8);
				var asses = Enumerable.Range(0, 28).Select(i =>
					new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory)
						.WithLayer(activity, new TimePeriod(8, 16))).ToArray();
				asses[5].SetDayOff(dayOffTemplate); //saturday
				asses[6].SetDayOff(dayOffTemplate);
				asses[12].SetDayOff(dayOffTemplate);
				asses[13].SetDayOff(dayOffTemplate);
				asses[19].SetDayOff(dayOffTemplate);
				asses[20].SetDayOff(dayOffTemplate);
				asses[26].SetDayOff(dayOffTemplate);
				asses[27].SetDayOff(dayOffTemplate);
				agentList.Add(agent);
				assesList.AddRange(asses);
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5,
				10, 7, 7, 7, 6, 5, 5);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agentList, assesList, skillDays);
			return stateHolder;
		}

		private double getResultingStandardDeviation(DateOnlyPeriod period, ISchedulerStateHolder stateHolder)
		{
			var allSkillDataExctractor =
				ScheduleResultDataExtractorProvider.CreateAllSkillsDataExtractor(period, stateHolder.SchedulingResultState,
					new AdvancedPreferences());
			var standardDeviation =
				Variances.StandardDeviation(allSkillDataExctractor.Values().Select(v => v.GetValueOrDefault(-1)));
			return standardDeviation;
		}

		public DayOffOptimizationDesktopQualityTest(RemoveImplicitResCalcContext removeImplicitResCalcContext, bool resourcePlannerDayOffOptimizationIslands47208) : base(removeImplicitResCalcContext, resourcePlannerDayOffOptimizationIslands47208)
		{
		}
	}
}