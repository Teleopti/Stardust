using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class SchedulingCascadingDesktopTest
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation()
		{
			const int numberOfAgents = 100;
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = DateOnly.Today;
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(7, 45, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(7, 45, 16, 0));
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)); //should not shovel resources here when deciding what shift to choose		
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			BusinessUnitRepository.HasCurrentBusinessUnit();
			var agents = new List<IPerson>();
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
				agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
				agent.Period(date).RuleSetBag = new RuleSetBag(ruleSet);
				agents.Add(agent);
			}
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), agents, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });
			
			Target.Execute(new OptimizerOriginalPreferences(new SchedulingOptions()),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);
		
			var allAssignmentsStartTime = schedulerStateHolder.Schedules.Select(keyValuePair => keyValuePair.Value).
				Select(range => range.ScheduledDay(date).PersonAssignment()).
				Select(x => x.Period.StartDateTime.TimeOfDay);

			allAssignmentsStartTime.Count().Should().Be.EqualTo(numberOfAgents);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
		}

		[Test]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation_TeamBlock()
		{
			const int numberOfAgents = 100;
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = DateOnly.Today; 
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(7, 45, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(7, 45, 16, 0));
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)); //should not shovel resources here when deciding what shift to choose		
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			BusinessUnitRepository.HasCurrentBusinessUnit();
			var agents = new List<IPerson>();
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				agent.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
				agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
				agent.Period(date).RuleSetBag = new RuleSetBag(ruleSet);
				agents.Add(agent);
			}
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), agents, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });
			var options = new SchedulingOptions {UseTeam = true, UseBlock = true};

			Target.Execute(new OptimizerOriginalPreferences(options),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			var allAssignmentsStartTime = schedulerStateHolder.Schedules.Select(keyValuePair => keyValuePair.Value).
				Select(range => range.ScheduledDay(date).PersonAssignment()).
				Select(x => x.Period.StartDateTime.TimeOfDay);

			allAssignmentsStartTime.Count().Should().Be.EqualTo(numberOfAgents);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
		}

		[Test]
		public void ShouldShovelWhenSchedulingHasBeenDone()
		{
			var date = DateOnly.Today;
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 0);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8,0,8,0,15), new TimePeriodWithSegment(16,0,16,0,15), new ShiftCategory("_").WithId()));
			BusinessUnitRepository.HasCurrentBusinessUnit();
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			agent.Period(date).RuleSetBag = new RuleSetBag(ruleSet);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent}, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });

			Target.Execute(new OptimizerOriginalPreferences(new SchedulingOptions()),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}
	}
}
