using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading.TrackShoveling;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationTrackShovelingTest : ResourceCalculationScenario
	{
		public ResourceCalculateWithNewContext Target;

		[Test]
		public void ShouldTrackShoveledToSubskillResources()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000,1,1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShovelingOneSkill(subskill, new DateTimePeriod(new DateTime(2000,1,1,8,0,0,DateTimeKind.Utc), new DateTime(2000,1,1,8,15,0,0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			var resourcesAdded = trackShoveling.AddedResources.Single();
			resourcesAdded.FromSkillSet.PrimarySkills.Should().Have.SameValuesAs(primarySkill);
			resourcesAdded.FromSkillSet.SubSkillsWithSameIndex.Single().Should().Have.SameValuesAs(subskill);
			resourcesAdded.ResourcesMoved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTrackShoveledFromPrimarySkillsResources()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShovelingOneSkill(primarySkill, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.RemovedResources.Single()
				.ResourcesMoved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTrackWhereResourcesWent()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShovelingOneSkill(primarySkill, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.RemovedResources.Single()
				.ToSubskills.Should().Have.SameValuesAs(subskill);
		}

		[Test]
		public void ShouldTrackResourcesBeforeShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShovelingOneSkill(primarySkill, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.ResourcesBeforeShoveling
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldTrackMultipleSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(new[] {primarySkill, subskill}, new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 8, 15, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.For(primarySkill).RemovedResources.Single()
				.ResourcesMoved.Should().Be.EqualTo(1);
			trackShoveling.For(subskill).AddedResources.Single()
				.ResourcesMoved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTrackResourcesBeforeShovelingMultipleSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8).DefaultResolution(15);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8).DefaultResolution(15);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(new[] { primarySkill, subskill }, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.For(primarySkill).ResourcesBeforeShoveling
				.Should().Be.EqualTo(2);
			trackShoveling.For(subskill).ResourcesBeforeShoveling
				.Should().Be.EqualTo(0);
		}
	}
}