using System;
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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class MinMaxStaffingDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		[Ignore("#44834")]
		public void ShouldRespectMinimumStaffingWhenBreakingShiftCategoryLimiation()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDayOne = skill.CreateSkillDayWithDemand(scenario, date, 10);
			var skillDayTwo = skill.CreateSkillDayWithDemand(scenario, date.AddDays(1), 1); //lower demand but min staffing
			var shiftCat = new ShiftCategory("_");
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCat) { MaxNumberOf = 1 }; //makes it "not legal"
			var agent = new Person().WithId()
								.InTimeZone(TimeZoneInfo.Utc)
								.WithPersonPeriod(skill)
								.WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var assDayOne = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCat);
			var assDayTwo = new PersonAssignment(agent, scenario, date.AddDays(1)).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCat);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date.AddDays(1)), new []{agent}, new[]{assDayOne, assDayTwo}, new[]{skillDayOne, skillDayTwo});
			foreach (var skillDataPeriod in skillDayTwo.SkillDataPeriodCollection)
			{
				skillDataPeriod.SkillPersonData = new SkillPersonData(1, 0); //at least one agent all daytwo
			}
			var schedulingOptions = new SchedulingOptions {UseMinimumPersons = true};

			Target.Execute(new NoSchedulingCallback(),
				schedulingOptions,
				new NoSchedulingProgress(),
				new []{agent}, 
				new DateOnlyPeriod(date, date.AddDays(1)),
				new OptimizationPreferences(),
				new DaysOffPreferences()
			);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.False();
			schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(1)).IsScheduled().Should().Be.True();
		}

		public MinMaxStaffingDesktopTest(bool resourcePlannerMergeTeamblockClassicScheduling44289, bool resourcePlannerSchedulingIslands44757) : base(resourcePlannerMergeTeamblockClassicScheduling44289, resourcePlannerSchedulingIslands44757)
		{
		}
	}
}