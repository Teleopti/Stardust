using System;
using System.Collections.Generic;
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
	//specific tests when ShovelResourcesFocusHighUnderstaffingPercentage is used for IShovelResourcesPerActivityIntervalSkillGroup
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class CascadingResourceCalculationFocusHighUnderstaffingPercentageTest : ISetup
	{
		public IResourceOptimizationHelper Target;

		[Test]
		public void ShouldPutMoreResourcesOnSkillsWithHighestUnderstaffPercentage()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 9, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB1 = new Skill("B1", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB1.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB1, new TimePeriod(8, 0, 9, 0));
			var skillDayB1 = skillB1.CreateSkillDayWithDemand(scenario, dateOnly, 21);
			var skillB2 = new Skill("B2", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB2.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB2, new TimePeriod(8, 0, 9, 0));
			var skillDayB2 = skillB2.CreateSkillDayWithDemand(scenario, dateOnly, 280);
			var agentA1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agentA1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB1, skillB2 });
			var assA1 = new PersonAssignment(agentA1, scenario, dateOnly);
			assA1.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
			var agentB1 = new Person().InTimeZone(TimeZoneInfo.Utc);
			agentB1.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillB1 });
			var assB1 = new PersonAssignment(agentB1, scenario, dateOnly);
			assB1.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
			var allAsses = new List<IPersonAssignment>();
			for (var i = 0; i < 200; i++)
			{
				var agentB2 = new Person().InTimeZone(TimeZoneInfo.Utc);
				agentB2.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillB2 });
				var assB2 = new PersonAssignment(agentB2, scenario, dateOnly);
				assB2.AddActivity(activity, new TimePeriod(8, 0, 9, 0));
				allAsses.Add(assB2);
			}
			allAsses.Add(assA1);
			allAsses.Add(assB1);

			Target.ResourceCalculate(dateOnly, ResourceCalculationDataCreator.WithData(scenario, dateOnly, allAsses, new[] { skillDayA, skillDayB1, skillDayB2 }, false, false));

			var b1AbsDiff = Math.Abs(skillDayB1.SkillStaffPeriodCollection.First().AbsoluteDifference);
			var b2AbsDiff = Math.Abs(skillDayB2.SkillStaffPeriodCollection.First().AbsoluteDifference);

			b1AbsDiff.Should().Be.LessThan(19.3);
			(b1AbsDiff + b2AbsDiff).Should().Be.IncludedIn(98.99, 99.01);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ShovelResourcesFocusHighUnderstaffingPercentage>().For<IShovelResourcesPerActivityIntervalSkillGroup>();
		}
	}
}