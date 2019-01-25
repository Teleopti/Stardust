using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
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
	public class DayOffOptimizationPreferenceDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;
		
		[TestCase(0.60, true, ExpectedResult = 2)]
		[TestCase(0.80, true, ExpectedResult = 1)]
		[TestCase(1, true, ExpectedResult = 0)]
		[TestCase(0.60, false, ExpectedResult = 2)]
		[TestCase(1, false, ExpectedResult = 2)]
		public int ShouldConsiderPreference(double preferencePercentage, bool usePreferences)
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = new Activity().WithId();
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				1, 1, 2, 2, 2, 2, 2);
			var presentShiftCategory = new ShiftCategory().WithId();
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(presentShiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			asses[6].SetDayOff(new DayOffTemplate()); //sunday
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = presentShiftCategory};
			IEnumerable<IScheduleData> preferenceDays = Enumerable.Range(0, 5).Select(x => new PreferenceDay(agent, date.AddDays(x), preferenceRestriction));
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, asses.Union(preferenceDays), skillDays);
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag(), UsePreferences = usePreferences, PreferencesValue = preferencePercentage} };
			
			Target.Execute(DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			return stateHolder.Schedules[agent].ScheduledDayCollection(new DateOnlyPeriod(date.AddDays(5), date.AddDays(6)))
				.Count(x => !x.HasDayOff()); //number of DOs moved
		}
		
		
		public DayOffOptimizationPreferenceDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}