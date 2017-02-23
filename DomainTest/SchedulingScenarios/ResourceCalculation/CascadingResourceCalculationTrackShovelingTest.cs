using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationTrackShovelingTest
	{
		public IResourceCalculation Target;

		[Test]
		public void ShouldTrackShoveledToSubskillResources()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000,1,1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(subskill, new DateTimePeriod(new DateTime(2000,1,1,8,0,0,DateTimeKind.Utc), new DateTime(2000,1,1,8,15,0,0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			var resourcesAdded = trackShoveling.AddedResources.Single();
			resourcesAdded.FromPrimarySkills.Should().Have.SameValuesAs(primarySkill);
			resourcesAdded.ResourcesMoved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTrackWhatParallellSkillsExistsWhenShovelingTo()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subSkill1 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var subSkill1Day = subSkill1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subSkill2 = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			var subSkill2Day = subSkill2.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subSkill1, subSkill2);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subSkill1, subSkill2);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(subSkill1, new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 8, 15, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkill1Day, subSkill2Day }, false, false));

			var resourcesAdded = trackShoveling.AddedResources.Single();
			resourcesAdded.ParallellSkills.Should().Have.SameValuesAs(subSkill2);
		}

		[Test]
		public void ShouldTrackShoveledFromPrimarySkillsResources()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(primarySkill, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			var resourcesRemoved = trackShoveling.RemovedResources.Single();
			resourcesRemoved.ResourcesMoved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTrackResourcesBeforeShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = new DateOnly(2000, 1, 1);
			var primarySkill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(7, 8);
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var subskill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(7, 8);
			var subSkillDay = subskill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent1 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, subskill);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).WithLayer(activity, new TimePeriod(5, 10));
			var trackShoveling = new TrackShoveling(primarySkill, new DateTimePeriod(new DateTime(2000, 1, 1, 7, 30, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 7, 45, 0, 0, DateTimeKind.Utc)));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(trackShoveling, scenario, dateOnly, new[] { ass1, ass2 }, new[] { primarySkillDay, subSkillDay }, false, false));

			trackShoveling.ResourcesBeforeShoveling
				.Should().Be.EqualTo(2);
		}
	}
}