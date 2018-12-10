using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingQuality
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayDesktopQualityTest : IntradayOptimizationScenarioTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public OptimizationDesktopExecuter Target;
		public IScheduleResultDataExtractorProvider ScheduleResultDataExtractorProvider;
		public ResourceCalculateWithNewContext ResourceCalculation;

		[Test]
		public void ShouldProduceGoodResultWithNoLimitations()
		{
			var date = new DateOnly(2017, 09, 04); //mon
			var agentList = new List<IPerson>();
			var stateHolder = setupStandardState(date, agentList);
			
			ResourceCalculation.ResourceCalculate(date, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
			var schedulingOptions = new SchedulingOptions();
			var p = ScheduleResultDataExtractorProvider
				.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(new[] {date}, schedulingOptions,
					stateHolder.SchedulingResultState);
			var valueBefore = p.Values().First();

			Target.Execute(new NoSchedulingProgress(), stateHolder, agentList, date.ToDateOnlyPeriod(), new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true} }, null);

			var valueAfter = p.Values().First();
			var valueImprovement = (valueBefore - valueAfter);
			valueImprovement.Should().Be.IncludedIn(0.51, 0.53);
		}

		private ISchedulerStateHolder setupStandardState(DateOnly date, ICollection<IPerson> agentList)
		{
			var activity = new Activity();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc)
				.IsOpen(new TimePeriod(8, 20))
				.DefaultResolution(60)
				.WithId();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var lunchActivity = new Activity();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 12, 0, 60),
				new TimePeriodWithSegment(16, 0, 20, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromMinutes(60)));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 2, 0, 60), new TimePeriodWithSegment(8, 0, 16, 0, 60)));
			var assesList = new List<IPersonAssignment>();
			for (var n = 0; n < 30; n++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, skill)
					.WithSchedulePeriodOneWeek(date, 1);
				agentList.Add(agent);
				assesList.Add(new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16)));
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
			var skillDays = skill.CreateSkillDayWithDemandPerHour(scenario, date, TimeSpan.Zero, demands);
			return SchedulerStateHolder.Fill(scenario, date, agentList, assesList, skillDays);
		}
	}
}