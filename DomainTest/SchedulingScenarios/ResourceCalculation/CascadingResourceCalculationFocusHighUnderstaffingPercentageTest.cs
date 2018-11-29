using System;
using System.Collections.Generic;
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
	//specific tests to ensure skills with high understaffing percentage are reduced quickly
	[DomainTest]
	public class CascadingResourceCalculationFocusHighUnderstaffingPercentageTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldPutMoreResourcesOnSkillsWithHighestUnderstaffPercentage()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 21);
			var skillB2 = new Skill("B2").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 280);
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB1, skillB2);
			var assA1 = new PersonAssignment(agentA1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB1);
			var assB1 = new PersonAssignment(agentB1, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9));
			var allAsses = new List<IPersonAssignment>();
			for (var i = 0; i < 200; i++)
			{
				var agentB2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillB2);
				allAsses.Add(new PersonAssignment(agentB2, scenario, dateOnly).WithLayer(activity, new TimePeriod(8, 9)));
			}
			allAsses.Add(assA1);
			allAsses.Add(assB1);

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, allAsses, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			var b1AbsDiff = Math.Abs(skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference);
			var b2AbsDiff = Math.Abs(skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference);

			b1AbsDiff.Should().Be.LessThan(19.3);
			(b1AbsDiff + b2AbsDiff).Should().Be.IncludedIn(98.99, 99.01);
		}
	}
}