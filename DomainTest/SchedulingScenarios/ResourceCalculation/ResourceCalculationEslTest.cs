using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest, Ignore("#40338")]
	public class ResourceCalculationEslTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public Func<IResourceOptimizationHelperExtended> ResourceOptimizationHelperExtended;

		[TestCase(2000, 0, 2000)]
		[TestCase(2000, 0.38, 3225)]
		public void ShouldCalculateEslCorrect(int demandedAgents, double shrinkage, int scheduledAgents)
		{
			const double expectedServiceLevel = 0.8;
			var scenario = new Scenario("_");
			var date = DateOnly.Today;
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
			{
				Activity = activity,
				TimeZone = TimeZoneInfo.Utc
			};
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(9, 0, 17, 0));
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, demandedAgents);
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
				skillStaffPeriod.Payload.Shrinkage = new Percent(shrinkage);
				skillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel.Percent = new Percent(expectedServiceLevel);
			}
			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < scheduledAgents; i++)
			{
				var agent = new Person().WithId();
				agent.AddPeriodWithSkill(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")),  new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
				var ass = new PersonAssignment(agent, scenario, date);
				ass.AddActivity(activity, new TimePeriod(9, 0, 17, 0));

				agents.Add(agent);
				asses.Add(ass);
			}
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(date, date), agents, asses, skillDay);

			ResourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			skillDay.SkillStaffPeriodCollection.First().EstimatedServiceLevelShrinkage.Value
				.Should().Be.IncludedIn(expectedServiceLevel-0.01, expectedServiceLevel+0.01);
		}
	}
}