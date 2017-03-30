using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingDesktopMaxSeatTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public IInitMaxSeatForStateHolder InitMaxSeatForStateHolder;

		public SchedulingDesktopMaxSeatTest(bool resourcePlannerTeamBlockPeriod42836) : base(resourcePlannerTeamBlockPeriod42836)
		{
		}

		[TestCase(true)]
		[TestCase(false)]
		[Toggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public void ShouldNotCareAboutMaxSeats(bool teamblock)
		{
			var schedulingOptions = new SchedulingOptions
			{
				UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak
			};
			if (teamblock)
			{
				schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy);
				schedulingOptions.UseTeam = true;
			}
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var scenario = new Scenario("_");
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(new TimePeriod(7, 45, 8, 0), TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(new TimePeriod(15, 45, 16, 0), TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			var team = new Team { Site = new Site("_") { MaxSeats = 0 }.WithId() };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneDay(date);

			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDay);

			Target.Execute(new OptimizerOriginalPreferences(schedulingOptions),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment(true).MainActivities()
				.Should().Not.Be.Empty();
		}

		[TestCase(true)]
		[TestCase(false)]
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public void ShouldCareAboutMaxSeatsWhenToggleIsOff(bool teamblock)
		{
			var schedulingOptions = new SchedulingOptions
			{
				UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak
			};
			if (teamblock)
			{
				schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy);
				schedulingOptions.UseTeam = true;
			}
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var scenario = new Scenario("_");
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(new TimePeriod(7, 45, 8, 0), TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(new TimePeriod(15, 45, 16, 0), TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			var team = new Team { Site = new Site("_") { MaxSeats = 0 }.WithId() };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneDay(date);

			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDay);
			InitMaxSeatForStateHolder.Execute(15);

			Target.Execute(new OptimizerOriginalPreferences(schedulingOptions),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment(true).MainActivities()
				.Should().Be.Empty();
		}
	}
}