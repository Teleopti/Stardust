using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class ResourceCalculationsInExtremeTimeZonesTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test, Ignore("bug #81679")]
		public void ShouldRemoveShiftToFulfillLimitationAndResourceCalculateCorrectIfExtremeDifferenceInAgentAndAgdasTimezone()
		{
			var startDate = new DateOnly(2018, 6, 11);
			var shiftCategory = new ShiftCategory().WithId();
			var scenario = new Scenario();
			var activity = new Activity();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen().WithId();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(startDate, 1), 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(new TimePeriod(23, 23), TimeSpan.FromMinutes(15)),
				new TimePeriodWithSegment(new TimePeriod(31, 31), TimeSpan.FromMinutes(15)), shiftCategory));
			var agent = new Person().WithSchedulePeriodOneWeek(startDate).WithPersonPeriod(ruleSet, skill)
				.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Aleutian Standard Time")).WithId();
			agent.SchedulePeriod(startDate).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory) {MaxNumberOf = 0});
			var assA = new PersonAssignment(agent, scenario, startDate.AddDays(5)).WithDayOff();
			var assB = new PersonAssignment(agent, scenario, startDate.AddDays(6)).WithDayOff();
			SchedulerStateHolderFrom.Fill(scenario, startDate.ToDateOnlyPeriod(), new[] {agent}, new[] {assA, assB}, skillDays);

			TimeZoneGuard.Set(TimeZoneInfo.FindSystemTimeZoneById("Aleutian Standard Time"));
			Target.Execute(new NoSchedulingCallback(),  new SchedulingOptions { UseShiftCategoryLimitations = false }, 
				new NoSchedulingProgress(), new[] {agent}, startDate.ToDateOnlyPeriod());
			skillDays[1].SkillStaffPeriodCollection
				.Any(skillStaffPeriod => skillStaffPeriod.CalculatedResource > 0)
				.Should().Be.True();

			TimeZoneGuard.Set(TimeZoneInfo.FindSystemTimeZoneById("Line Islands Standard Time"));
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions { UseShiftCategoryLimitations = true },
				new NoSchedulingProgress(), new[] {agent}, startDate.ToDateOnlyPeriod());
			skillDays[1].SkillStaffPeriodCollection
				.Any(skillStaffPeriod => skillStaffPeriod.CalculatedResource > 0)
				.Should().Be.False();
		}

		public ResourceCalculationsInExtremeTimeZonesTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}