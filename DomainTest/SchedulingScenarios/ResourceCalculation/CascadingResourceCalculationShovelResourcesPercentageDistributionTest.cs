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
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	//specific tests when ShovelResourcesPercentageDistribution is used for IShovelResourcesPerActivityIntervalSkillGroup
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class CascadingResourceCalculationShovelResourcesPercentageDistributionTest : ISetup
	{
		public IResourceOptimizationHelper Target;

		[Test]
		public void ShouldMoveResourcesToTwoSkillsWithDifferentDemandBasedOnPercentageDistribution()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0.5);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB1, skillB2 });
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new TimePeriod(5, 0, 10, 0));

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, new[] { ass }, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			var b1Understaffed = -skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference;
			var b2Understaffed = -skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference;
			(Math.Abs(b1Understaffed * 2 - b2Understaffed) < 0.0001).Should().Be.True();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ShovelResourcesPercentageDistribution>().For<IShovelResourcesPerActivityIntervalSkillGroup>();
		}
	}
}