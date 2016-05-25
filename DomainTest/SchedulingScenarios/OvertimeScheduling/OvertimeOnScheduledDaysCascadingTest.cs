using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class OvertimeOnScheduledDaysCascadingTest
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
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
			var skillADay = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 2);
			var skillB = new Skill("B", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var skillBDay = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 2);

			var agentKnowingSkillAandB1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB1.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB1.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));

			var agentKnowingSkillAandB2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentKnowingSkillAandB2.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA, skillB });
			agentKnowingSkillAandB2.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));

			var agentThatShouldNotGetOverTime = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agentThatShouldNotGetOverTime.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }),
				new[] { skillA});
			agentKnowingSkillAandB2.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1));

			var assAandB1 = new PersonAssignment(agentKnowingSkillAandB1, scenario, dateOnly);
			assAandB1.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var assAandB2 = new PersonAssignment(agentKnowingSkillAandB2, scenario, dateOnly);
			assAandB2.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			var scheduleNotToTouch = new PersonAssignment(agentThatShouldNotGetOverTime, scenario, dateOnly);
			scheduleNotToTouch.AddActivity(activity, new TimePeriod(8, 0, 15, 0));

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agentKnowingSkillAandB1, agentKnowingSkillAandB2, agentThatShouldNotGetOverTime }, new[] { assAandB1, assAandB2, scheduleNotToTouch }, new[] { skillADay, skillBDay });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(15, 0, 16, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agentThatShouldNotGetOverTime].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities()
				.Should().Be.Empty();
		}
	}
}