using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationRestrictionsDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizeIntradayIslandsDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		
		[Test]
		public void ShouldConsiderRotations()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new RotationRestriction {ShiftCategory = shiftCategory};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseRotations = true;
			optimizationPreferences.General.RotationsValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderAvailabilities()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new AvailabilityRestriction {NotAvailable = true};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UseAvailabilities = true;
			optimizationPreferences.General.AvailabilitiesValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderPreferences()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.UsePreferences = true;
			optimizationPreferences.General.PreferencesValue = 1.0d;

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), optimizationPreferences, new NoIntradayOptimizationCallback());

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		
		public IntradayOptimizationRestrictionsDesktopTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, bool resourcePlannerSpeedUpShiftsWithinDay45694) : base(resourcePlannerMergeTeamblockClassicIntraday45508, resourcePlannerSpeedUpShiftsWithinDay45694)
		{
		}
	}
}