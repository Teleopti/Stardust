using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationSmallOverstaffingTests : ResourceCalculationScenario
	{
		//tests are based on limit on 0.1 -> if that changes, tests need to be rewritten
		//(don't know if it's better to fake the value, but ended up not doing so...)
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldNotShovelIfTooSmallOverstaff()
		{
			const double primarySkillDemand = 0.91;
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var subSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var subSkillDay = subSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkillDay, subSkillDay }, false, false));

			var primarySkillDiff = primarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference;
			const double expectedPrimaryDiff = -primarySkillDemand + 1;
			Math.Abs(primarySkillDiff - expectedPrimaryDiff).Should().Be.LessThan(0.001); //rounding issues
			subSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldShovelIfSumOfOverstaffingIsEnough()
		{
			var primarySkillDemand = 0.1 * 0.8;
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var primarySkill1Day = primarySkill1.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var primarySkill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var primarySkill2Day = primarySkill2.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var subSkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var subSkillDay = subSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill1, primarySkill2, subSkill);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkill1Day, primarySkill2Day, subSkillDay }, false, false));

			primarySkill1Day.SkillStaffPeriodCollection.First().AbsoluteDifference.IsZero()
				.Should().Be.True();
			primarySkill2Day.SkillStaffPeriodCollection.First().AbsoluteDifference.IsZero()
				.Should().Be.True();
			subSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.GreaterThan(-1);
		}
	}
}