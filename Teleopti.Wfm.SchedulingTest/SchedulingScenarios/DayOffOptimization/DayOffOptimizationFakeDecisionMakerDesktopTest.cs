using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
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

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationFakeDecisionMakerDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;

		[Test]
		[Timeout(10000)]
		public void ShouldNotHangIfDecisionMakerReturnsTrueButBitArraysAreTheSame()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } };
			var dayOffPrefs = new DaysOffPreferences {ConsecutiveDaysOffValue = new MinMax<int>(1, 10)};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffPrefs), new NoOptimizationCallback());

			//"Assert" by timeout attribute
		}

		public override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<decisionMakerFactoryForTest>().For<IDayOffOptimizationDecisionMakerFactory>();
		}

		private class decisionMakerFactoryForTest : IDayOffOptimizationDecisionMakerFactory
		{
			public IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(ILockableBitArray scheduleMatrixArray, IOptimizationPreferences optimizerPreferences,
				IDaysOffPreferences daysOffPreferences)
			{
				return new[] {new decisionMakerReturningTrueButNotModifyingBitArray()};
			}
		}

		private class decisionMakerReturningTrueButNotModifyingBitArray : IDayOffDecisionMaker
		{
			public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
			{
				return true;
			}
		}

		public DayOffOptimizationFakeDecisionMakerDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}