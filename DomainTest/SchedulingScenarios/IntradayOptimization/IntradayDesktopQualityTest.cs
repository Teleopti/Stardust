using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayDesktopQualityTest : IntradayOptimizationScenarioTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IIntradayOptimization Target;
		public IScheduleResultDataExtractorProvider ScheduleResultDataExtractorProvider;
		public IUserTimeZone UserTimeZone;

		[Test]
		public void ShouldProduceGoodResultWithNoLimitations()
		{
			var stateHolder = setupStandardState(out var period, out var agentList);
			Target.Execute(period, agentList, false);
			var result = ScheduleResultDataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(period.DayCollection(),
				new SchedulingOptions(), stateHolder.SchedulingResultState);
			result.Values().First().Should().Be.LessThan(0.28);
		}

		private ISchedulerStateHolder setupStandardState(out DateOnlyPeriod period, out List<IPerson> agentList)
		{
			var firstDay = new DateOnly(2017, 09, 04); //mon
			period = firstDay.ToDateOnlyPeriod();
			var activity = new Activity("_"){InContractTime = true};
			var skill = new Skill().For(activity).InTimeZone(UserTimeZone.TimeZone())
				.IsOpen(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(20)));
			skill.DefaultResolution = 60;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var lunchActivity = new Activity("l"){RequiresSkill = false, InContractTime = true};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 12, 0, 60),
				new TimePeriodWithSegment(16, 0, 20, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromMinutes(60)));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 2, 0, 60),
				new TimePeriodWithSegment(8, 0, 16, 0, 60)));
			agentList = new List<IPerson>();
			var assesList = new List<IPersonAssignment>();
			var dayOffTemplate = new DayOffTemplate();
			for (var n = 0; n < 30; n++)
			{
				var agent = new Person().WithId().InTimeZone(UserTimeZone.TimeZone()).WithPersonPeriod(ruleSet, skill);
				agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));
				agent.SchedulePeriod(firstDay).SetDaysOff(2);
				//var ass = new PersonAssignment(agent, scenario, firstDay).ShiftCategory(shiftCategory)
				//	.WithLayer(activity, new TimePeriod(8, 16));
				var asses = Enumerable.Range(0, 7).Select(i =>
					new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory)
						.WithLayer(activity, new TimePeriod(8, 16))).ToArray();
				asses[5].SetDayOff(dayOffTemplate); //saturday
				asses[6].SetDayOff(dayOffTemplate);
				agentList.Add(agent);
				assesList.AddRange(asses);
			}

			var demands = new List<Tuple<int, TimeSpan>>
			{
				new Tuple<int, TimeSpan>(8, TimeSpan.FromHours(12)),
				new Tuple<int, TimeSpan>(9, TimeSpan.FromHours(15)),
				new Tuple<int, TimeSpan>(10, TimeSpan.FromHours(25)),
				new Tuple<int, TimeSpan>(11, TimeSpan.FromHours(35)),
				new Tuple<int, TimeSpan>(12, TimeSpan.FromHours(40)),
				new Tuple<int, TimeSpan>(13, TimeSpan.FromHours(38)),
				new Tuple<int, TimeSpan>(14, TimeSpan.FromHours(30)),
				new Tuple<int, TimeSpan>(15, TimeSpan.FromHours(20)),
				new Tuple<int, TimeSpan>(16, TimeSpan.FromHours(12)),
				new Tuple<int, TimeSpan>(17, TimeSpan.FromHours(8)),
				new Tuple<int, TimeSpan>(18, TimeSpan.FromHours(5)),
				new Tuple<int, TimeSpan>(19, TimeSpan.FromHours(3))
			};
			var skillDays = skill.CreateSkillDayWithDemandPerHour(scenario, firstDay, TimeSpan.Zero, demands);

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agentList, assesList, skillDays);
			return stateHolder;
		}

		public IntradayDesktopQualityTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508) : base(resourcePlannerMergeTeamblockClassicIntraday45508)
		{
		}
	}
}