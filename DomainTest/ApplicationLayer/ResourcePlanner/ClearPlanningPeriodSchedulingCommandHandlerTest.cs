using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[TestFixture]
	[DomainTest]
	public class ClearPlanningPeriodSchedulingCommandHandlerTest
	{
		public ClearPlanningPeriodSchedulingCommandHandler Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeAgentGroupRepository AgentGroupRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeMeetingRepository MeetingRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		[Test]
		public void ShouldClearAssignmentWithinRange()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			AssignmentRepository.Has(agent, scenario, activity, shiftCategory, startDate, new TimePeriod(8, 16));

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] {agent}, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotClearAssignmentOutsideRange()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDateOutsidePlanningPeriod = new DateOnly(2017, 05, 08);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			AssignmentRepository.Has(agent, scenario, activity, shiftCategory, endDateOutsidePlanningPeriod, new TimePeriod(8, 16));

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDateOutsidePlanningPeriod), scenario);
			assignments.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotClearAssignmentForPersonOutsideAgentGroup()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team().WithId(),
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			AssignmentRepository.Has(agent, scenario, activity, shiftCategory, startDate, new TimePeriod(8, 16));

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotClearAbsence()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			AssignmentRepository.Has(agent, scenario, activity, shiftCategory, startDate, new TimePeriod(8, 16));
			PersonAbsenceRepository.Has(new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(new DateTime(2017, 05, 01, 8, 0, 0, DateTimeKind.Utc), new DateTime(2017, 05, 01, 16, 0, 0, DateTimeKind.Utc)))));

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Should().Be.Empty();

			var absences = PersonAbsenceRepository.Find(new[] {agent}, new DateTimePeriod(new DateTime(2017, 05, 01, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 05, 07, 0, 0, 0, DateTimeKind.Utc)));
			absences.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotClearPersonalActivity()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			var assignment = new PersonAssignment(agent, scenario, startDate);
			assignment.AddActivity(activity, new TimePeriod(9, 16));
			assignment.AddPersonalActivity(activity, new DateTimePeriod(2017, 05, 01, 8, 2017, 05, 01, 9));
			assignment.SetShiftCategory(shiftCategory);
			AssignmentRepository.Has(assignment);

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Count.Should().Be.EqualTo(1);
			assignments.First().ShiftLayers.Count().Should().Be.EqualTo(1);
			assignments.First().ShiftLayers.First().Should().Be.OfType<PersonalShiftLayer>();
		}

		[Test]
		public void ShouldClearDayOffButNotPersonalActivity()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var dayOffTemplate = new DayOffTemplate().WithId();
			DayOffTemplateRepository.Has(dayOffTemplate);
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			var assignment = new PersonAssignment(agent, scenario, startDate);
			assignment.SetDayOff(dayOffTemplate);
			assignment.AddPersonalActivity(activity, new DateTimePeriod(2017, 05, 01, 8, 2017, 05, 01, 9));
			AssignmentRepository.Has(assignment);

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Count.Should().Be.EqualTo(1);
			assignments.First().ShiftLayers.Count().Should().Be.EqualTo(1);
			assignments.First().ShiftLayers.First().Should().Be.OfType<PersonalShiftLayer>();
			assignments.First().AssignedWithDayOff(dayOffTemplate).Should().Be.False();
		}

		[Test]
		public void ShouldNotClearMeeting()
		{
			var startDate = new DateOnly(2017, 05, 01);
			var endDate = new DateOnly(2017, 05, 07);
			var team = new Team().WithId();
			var activity = ActivityRepository.Has("_");
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var skill = SkillRepository.Has("skill", activity);
			var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team,
				new SchedulePeriod(startDate, SchedulePeriodType.Week, 1), ruleSet, skill);
			var agentGroup = new AgentGroup("_").WithId().AddFilter(new TeamFilter(team));
			AgentGroupRepository.Has(agentGroup);
			var planningPeriod = PlanningPeriodRepository.Has(startDate, 1, SchedulePeriodType.Week, agentGroup);

			AssignmentRepository.Has(agent, scenario, activity, shiftCategory, startDate, new TimePeriod(8, 16));
			MeetingRepository.Has(new Meeting(agent, new []{new MeetingPerson(agent, false) }, "_", "_", "_", activity, scenario)
			{
				StartDate = startDate,
				StartTime = TimeSpan.FromHours(8),
				EndDate = startDate,
				EndTime = TimeSpan.FromHours(10)
			});

			Target.ClearSchedules(new ClearPlanningPeriodSchedulingCommand
			{
				PlanningPeriodId = planningPeriod.Id.GetValueOrDefault()
			});

			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.New);

			var assignments = AssignmentRepository.Find(new[] { agent }, new DateOnlyPeriod(startDate, endDate), scenario);
			assignments.Should().Be.Empty();

			var meetings = MeetingRepository.Find(new[] {agent}, new DateOnlyPeriod(startDate, endDate), scenario);
			meetings.Count.Should().Be.EqualTo(1);
		}
	}
}