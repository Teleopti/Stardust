﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationMinimumAgentsDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;
		
		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void ShouldMoveDayOffToDayWithLessDemand(bool useMinimumStaffing, int expectedDayWithDO)
		{
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity("_");
			var skill = new Skill().WithId().For(activity).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,
				0.8, //minimum staffing = 1, prevent putting DO here
				0.9, //DO should end up here
				1,
				1,
				1,
				1, //DO from beginning
				1);
			skillDays.First().SetMinimumAgents(new TimePeriod(8, 16), 1);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag()}, 
				Advanced = {UseMinimumStaffing = useMinimumStaffing}
			};

			Target.Execute(period, new[] { agent }, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), (o, args) => {}, new NoOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDayCollection(period).Single(x => x.PersonAssignment().DayOff() != null).PersonAssignment().Date
				.Should().Be.EqualTo(skillDays[expectedDayWithDO].CurrentDate); 
		}

		public DayOffOptimizationMinimumAgentsDesktopTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerDayOffOptimizationIslands47208, bool resourcePlannerMinimumAgents75339, bool resourcePlannerLessResourcesXXL74915) : base(seperateWebRequest, resourcePlannerDayOffOptimizationIslands47208, resourcePlannerMinimumAgents75339, resourcePlannerLessResourcesXXL74915)
		{
			if(!ResourcePlannerMinimumAgents75339)
				Assert.Ignore("Only available when ResourcePlanner_MinimumAgents_75339 is true");
		}
	}
}