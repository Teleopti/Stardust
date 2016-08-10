using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class CascadingResourceCalculationSmallOverstaffingTests
	{
		//tests are based on limit on 0.1 -> if that changes, tests need to be rewritten
		//(don't know if it's better to fake the value, but ended up not doing so...)
		public IResourceOptimizationHelper Target;

		[Test]
		public void ShouldNotShovelIfTooSmallOverstaff()
		{
			const double primarySkillDemand = 0.91;
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(primarySkill, new TimePeriod(8, 0, 9, 0));
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var subSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			subSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(subSkill, new TimePeriod(8, 0, 9, 0));
			var subSkillDay = subSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { primarySkill, subSkill });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

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
			var primarySkill1 = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill1.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(primarySkill1, new TimePeriod(8, 0, 9, 0));
			var primarySkill1Day = primarySkill1.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var primarySkill2 = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill2.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(primarySkill2, new TimePeriod(8, 0, 9, 0));
			var primarySkill2Day = primarySkill2.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var subSkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			subSkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(subSkill, new TimePeriod(8, 0, 9, 0));
			var subSkillDay = subSkill.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { primarySkill1, primarySkill2, subSkill });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { primarySkill1Day, primarySkill2Day, subSkillDay }, false, false));

			primarySkill1Day.SkillStaffPeriodCollection.First().AbsoluteDifference.IsZero()
				.Should().Be.True();
			primarySkill2Day.SkillStaffPeriodCollection.First().AbsoluteDifference.IsZero()
				.Should().Be.True();
			subSkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.GreaterThan(-1);
		}

		[Test]
		public void ShouldStopShovelWhenLimitIsReached()
		{
			const double primarySkillDemand = 0.99 - 0.1; //one loop only
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var primarySkill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(primarySkill, new TimePeriod(8, 0, 9, 0));
			var primarySkillDay = primarySkill.CreateSkillDayWithDemand(scenario, dateOnly, primarySkillDemand);
			var subSkill1 = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			subSkill1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(subSkill1, new TimePeriod(8, 0, 9, 0));
			var subSkill1Day = subSkill1.CreateSkillDayWithDemand(scenario, dateOnly, 0.01); //rel diff 1 -> most of resources will be put here first loop
			var subSkill2 = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			subSkill2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(subSkill2, new TimePeriod(8, 0, 9, 0));
			var subSkill2Day = subSkill2.CreateSkillDayWithDemand(scenario, dateOnly, 1.1); //rel diff 0.1 -> just 0.1/1.1 of resources will be put here first loop
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), 
				new[] { primarySkill, subSkill1, subSkill2 });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { subSkill2 });
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(8, 0, 9, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass, ass2 }, new[] { primarySkillDay, subSkill1Day, subSkill2Day }, false, false));

			var primarySkillDiff = primarySkillDay.SkillStaffPeriodCollection.First().AbsoluteDifference;
			primarySkillDiff.IsZero().Should().Be.False();
			primarySkillDiff.Should().Be.LessThanOrEqualTo(0.1);
			var subSkill2Diff = subSkill2Day.SkillStaffPeriodCollection.First().AbsoluteDifference;
			subSkill2Diff.IsZero().Should().Be.False();
		}
	}
}