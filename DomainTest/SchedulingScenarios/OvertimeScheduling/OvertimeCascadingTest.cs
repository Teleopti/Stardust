using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTestWithStaticDependenciesAvoidUse]
	public class OvertimeCascadingTest
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotShovelAfterOvertimeHasBeenPlaced()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_").WithId();
			var today = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, today, 0);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, today, 1);
			var agentKnowingSkillAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB.AddPeriodWithSkills(new PersonPeriod(today, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB.AddSchedulePeriod(new SchedulePeriod(today, SchedulePeriodType.Day, 1));
			var agentKnowingSkillB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillB.AddPeriodWithSkills(new PersonPeriod(today, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillB });
			agentKnowingSkillB.AddSchedulePeriod(new SchedulePeriod(today, SchedulePeriodType.Day, 1));
			var assForAandB = new PersonAssignment(agentKnowingSkillAandB, scenario, today);
			assForAandB.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, today.ToDateOnlyPeriod(), new[] { agentKnowingSkillAandB, agentKnowingSkillB }, new[] { assForAandB }, new[] { skillADay, skillBDay });
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var overtimePreference = new OvertimePreferences { OvertimeType = definitionSet, ShiftBagToUse = new RuleSetBag(ruleSet), ScheduleTag = new ScheduleTag() };

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentKnowingSkillB].ScheduledDay(today) });

			stateHolder.Schedules[agentKnowingSkillB].ScheduledDay(today).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}
	}
}