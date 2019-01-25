using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationTest : IExtendSystem
	{
		public ResourceCalculateWithNewContext ResourceCalculateInContext;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test]
		public void ShouldSplitResourceEqualIfEfficiencyIs100AndDemandIsEqual()
		{
			// agent 1, 100%,  100%
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016,5,12);
			var commonTimeZone = TimeZoneGuard.CurrentTimeZone();

			var skillA = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillA);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillB = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillB);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));

			var agent = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> {skillA, skillB});
			agent.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(0,0,0,30));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] {agent},
				new List<IScheduleData> { ass }, new List<ISkillDay> {skillDayA, skillDayB});

			ResourceCalculateInContext.ResourceCalculate(dateOnly, stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false));
			var intervalA = stateHolder.SchedulingResultState.SkillDays[skillA].First().SkillStaffPeriodCollection.First();
			var intervalB = stateHolder.SchedulingResultState.SkillDays[skillB].First().SkillStaffPeriodCollection.First();

			intervalA.CalculatedResource.Should().Be.EqualTo(intervalB.CalculatedResource);
			intervalA.CalculatedResource.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldSplitResourceEqualIfEfficiencyIs100AndDemandIsEqualWithTwoAgentsOneSingleSkilled()
		{
			// agent 1, 100%,  100%
			// agent 2, 100%
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016, 5, 12);
			var commonTimeZone = TimeZoneGuard.CurrentTimeZone();

			var skillA = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillA);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillB = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillB);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA, skillB });
			agent1.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA });
			agent2.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 },
				new List<IScheduleData> { ass1, ass2 }, new List<ISkillDay> { skillDayA, skillDayB });

			ResourceCalculateInContext.ResourceCalculate(dateOnly, stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false));
			var intervalA = stateHolder.SchedulingResultState.SkillDays[skillA].First().SkillStaffPeriodCollection.First();
			var intervalB = stateHolder.SchedulingResultState.SkillDays[skillB].First().SkillStaffPeriodCollection.First();

			Math.Round(intervalA.CalculatedResource, 2).Should().Be.EqualTo(Math.Round(intervalB.CalculatedResource, 2));
			Math.Round(intervalA.CalculatedResource, 2).Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldSplitResourceAccordinglyIfEfficiencyIs100And50AndDemandIsEqual()
		{
			// agent 1, 100%,  50%
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016, 5, 12);
			var commonTimeZone = TimeZoneGuard.CurrentTimeZone();

			var skillA = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillA);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillB = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillB);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));

			var agent = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA, skillB });
			agent.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var personSkillB = (IPersonSkillModify)agent.Period(dateOnly).PersonSkillCollection.First(ps => ps.Skill.Equals(skillB));
			personSkillB.SkillPercentage = new Percent(0.5);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent },
				new List<IScheduleData> { ass }, new List<ISkillDay> { skillDayA, skillDayB });

			ResourceCalculateInContext.ResourceCalculate(dateOnly, stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false));
			var intervalA = stateHolder.SchedulingResultState.SkillDays[skillA].First().SkillStaffPeriodCollection.First();
			var intervalB = stateHolder.SchedulingResultState.SkillDays[skillB].First().SkillStaffPeriodCollection.First();

			intervalA.CalculatedResource.Should().Be.EqualTo(0.5);
			intervalB.CalculatedResource.Should().Be.EqualTo(0.25);
		}

		[Test]
		public void ShouldSplitResourceAccordinglyIfEfficiencyIs100And50AndDemandIsEqualWithTwoAgentsOneSingleSkilled()
		{
			// agent 1, 100%,  50%
			// agent 2, 100%
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016, 5, 12);
			var commonTimeZone = TimeZoneGuard.CurrentTimeZone();

			var skillA = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillA);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillB = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillB);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA, skillB });
			var personSkillB = (IPersonSkillModify)agent1.Period(dateOnly).PersonSkillCollection.First(ps => ps.Skill.Equals(skillB));
			personSkillB.SkillPercentage = new Percent(0.5);
			agent1.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA });
			agent2.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 },
				new List<IScheduleData> { ass1, ass2 }, new List<ISkillDay> { skillDayA, skillDayB });

			ResourceCalculateInContext.ResourceCalculate(dateOnly, stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false));
			var intervalA = stateHolder.SchedulingResultState.SkillDays[skillA].First().SkillStaffPeriodCollection.First();
			var intervalB = stateHolder.SchedulingResultState.SkillDays[skillB].First().SkillStaffPeriodCollection.First();

			Math.Round(intervalA.CalculatedResource, 2).Should().Be.EqualTo(1);
			Math.Round(intervalB.CalculatedResource, 2).Should().Be.EqualTo(0.5);
			// You can argue that intervalB should be higher than 0.5 as agent1 will spend almost all his time on B. In this case we have lost 0.5 resources in total
		}

		[Test]
		public void ShouldSplitResourceAccordinglyIfEfficiencyIs100And50AndDemandIsEqualWithTwoAgents()
		{
			// agent 1, 100%,  50%
			// agent 2, 100%, 100%
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2016, 5, 12);
			var commonTimeZone = TimeZoneGuard.CurrentTimeZone();

			var skillA = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillA);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillB = new Skill("_", "_", Color.Empty, 30, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = commonTimeZone }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillB);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA, skillB });
			var personSkillB = (IPersonSkillModify)agent1.Period(dateOnly).PersonSkillCollection.First(ps => ps.Skill.Equals(skillB));
			personSkillB.SkillPercentage = new Percent(0.5);
			agent1.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass1 = new PersonAssignment(agent1, scenario, dateOnly);
			ass1.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill> { skillA, skillB });
			agent2.PermissionInformation.SetDefaultTimeZone(commonTimeZone);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly);
			ass2.AddActivity(phoneActivity, new TimePeriod(0, 0, 0, 30));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent1, agent2 },
				new List<IScheduleData> { ass1, ass2 }, new List<ISkillDay> { skillDayA, skillDayB });

			ResourceCalculateInContext.ResourceCalculate(dateOnly, stateHolder.SchedulingResultState.ToResourceOptimizationData(false, false));
			var intervalA = stateHolder.SchedulingResultState.SkillDays[skillA].First().SkillStaffPeriodCollection.First();
			var intervalB = stateHolder.SchedulingResultState.SkillDays[skillB].First().SkillStaffPeriodCollection.First();

			Math.Round(intervalA.CalculatedResource, 2).Should().Be.EqualTo(1);
			Math.Round(intervalB.CalculatedResource, 2).Should().Be.EqualTo(0.75);
			// You can argue that intervalB should be higher than 0.75 as agent1 could spend almost all his time on A. In this case we have lost 0.25 resources in total
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

	}
}