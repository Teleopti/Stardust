using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnScheduleDaysTest
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IGridlockManager LockManager;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test]
		public void ShouldSkipLockedDaysWithAssignmentWithLayers()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			SkillDayRepository.Has(new List<ISkillDay>{skillDay});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 16, 0));
			var ass = PersonAssignmentRepository.GetSingle(dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};

			LockManager.AddLock(agent, dateOnly,LockType.Normal, new DateTimePeriod());

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });
			
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Be.Empty();
		}

		[Test, Ignore]
		public void ShouldPlaceOvertimeWhenViewerAndAgentTimeZonesAreFarAway()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(23, 15, 23, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agentUserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
			var endUserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
			
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), 
											new PartTimePercentage("_"), 
											new Team { Site = new Site("site") }, schedulePeriod, 
											ruleSet, skill)
											.InTimeZone(agentUserTimeZone);

			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(22, TimeSpan.FromMinutes(360)));
			SkillDayRepository.Has(new List<ISkillDay> { skillDay });
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(23, 15, 31, 15));
			var ass = PersonAssignmentRepository.GetSingle(dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 30, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};

			TimeZoneGuard.SetTimeZone(endUserTimeZone);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}
	}
}