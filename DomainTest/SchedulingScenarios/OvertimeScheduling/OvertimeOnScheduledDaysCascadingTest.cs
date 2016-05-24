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
using Teleopti.Ccc.Domain.Scheduling.TimeLayer; 
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnScheduledDaysCascadingTest
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test, Ignore]
		public void ShouldNotPlaceOvertimeShiftDueToNoUnderstaffingAfterShoveling()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_").WithId();
			var dateOnly = DateOnly.Today;
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(16)) };
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var skillA = new Skill("A", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agentThatShouldGetNoOvertime = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentThatShouldGetNoOvertime.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentThatShouldGetNoOvertime.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			var agentKnowingSkillAandB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			var agentKnowingSkillB = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillB.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillB });
			agentKnowingSkillB.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));
			var ass1 = new PersonAssignment(agentKnowingSkillAandB, scenario, dateOnly);
			ass1.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var ass2 = new PersonAssignment(agentKnowingSkillB, scenario, dateOnly);
			ass2.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var agentThatShouldGetNoOvertimeSchedule = new PersonAssignment(agentThatShouldGetNoOvertime, scenario, dateOnly);
			agentThatShouldGetNoOvertimeSchedule.AddActivity(activity, new TimePeriod(8, 0, 15, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentKnowingSkillAandB, agentKnowingSkillB, agentThatShouldGetNoOvertime }, new[] { ass1, ass2, agentThatShouldGetNoOvertimeSchedule }, new[] { skillADay, skillBDay });

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 16, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};
			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentThatShouldGetNoOvertime].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentThatShouldGetNoOvertime].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}
	}
}