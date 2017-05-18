using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingRestrictionsDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldNotScheduleShiftsForRestrictionsOnlyWhenNoRestrictionExists()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var otherActivity = new Activity("_").WithId();
			otherActivity.RequiresSkill = true;
			var scenario = new Scenario("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 100);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())) { OnlyForRestrictions = true };
			var rulsetToGetMinMaxWorkTimeFrom = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(otherActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())); //so we get min max worktime from the ruleset bag when not having any availability restriction
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			agent.Period(date).RuleSetBag.AddRuleSet(rulsetToGetMinMaxWorkTimeFrom);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), new[] { skillDay });
			var schedulingOptions = new SchedulingOptions { UseAvailability = true };

			Target.Execute(schedulingOptions,
				new NoSchedulingProgress(),
				schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.False();
		}

		public SchedulingRestrictionsDesktopTest(bool resourcePlannerTeamBlockPeriod42836, bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerTeamBlockPeriod42836, resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}
