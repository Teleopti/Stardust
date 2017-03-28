using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	//TODO: before adding more tests here remove mess with mix of repos and stateholder
	[DomainTest]
	public class BackToLegalShiftCommandTest
	{
		public BackToLegalShiftCommand Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IFillSchedulerStateHolder FillSchedulerStateHolder;

		[Test]
		public void ShouldRestoreToLegalShiftBagShiftWithSameStartAndEndTime()
		{
			var team = new Team().WithDescription(new Description("team"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_");
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);

			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(1));
			AssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("_"), firstDay.AddDays(2),
				new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));

			FillSchedulerStateHolder.Fill(SchedulerStateHolder(),null,null,null, period);
			var scheduleDays = new List<IScheduleDay>
			{
				SchedulerStateHolder().Schedules[agent1].ScheduledDay(firstDay.AddDays(2))
			};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, SchedulerStateHolder().SchedulingResultState, SchedulerStateHolder().AllPermittedPersons);

			var newAssignment = SchedulerStateHolder().Schedules[agent1].ScheduledDay(firstDay.AddDays(2)).PersonAssignment();
			newAssignment.ShiftLayers.Count().Should().Be.EqualTo(2);
			newAssignment.ShiftLayers.First()
				.Period.StartDateTime.Should()
				.Be.EqualTo(scheduleDays.First().PersonAssignment().ShiftLayers.First().Period.StartDateTime);
		}

		[Test]
		public void ShouldKeepOvertimeWhenRestoreToLegalShiftBagShiftWithSameStartAndEndTime()
		{
			var team = new Team().WithDescription(new Description("team"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_");
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var contract = new Contract("_");
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);

			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(1));
			AssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("_"), firstDay.AddDays(2), new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
			var ass = AssignmentRepository.GetSingle(firstDay.AddDays(2), agent1);
			var dateTimePeriod = new DateTimePeriod(ass.Period.StartDateTime.AddHours(-1), ass.Period.StartDateTime);
			ass.AddOvertimeActivity(phoneActivity,dateTimePeriod, definitionSet);

			FillSchedulerStateHolder.Fill(SchedulerStateHolder(), null, null, null, period);
			var scheduleDays = new List<IScheduleDay>
			{
				SchedulerStateHolder().Schedules[agent1].ScheduledDay(firstDay.AddDays(2))
			};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, SchedulerStateHolder().SchedulingResultState, SchedulerStateHolder().AllPermittedPersons);

			SchedulerStateHolder().Schedules[agent1].ScheduledDay(firstDay.AddDays(2)).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldFindLegalShiftWhenNotAllowPersonalActivityOverwriteActivity()
		{
			var team = new Team().WithDescription(new Description("team"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var firstDay = new DateOnly(2015, 10, 12);
			var secondDay = firstDay.AddDays(1);
			var thirdDay = firstDay.AddDays(2);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_") {AllowOverwrite = true};
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var personalActivity = new Activity("personal") {RequiresSkill = false};
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(0, 15, 0, 15, 15), new TimePeriodWithSegment(0, 0, 0, 15, 15)));
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);

			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, secondDay);
			AssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, thirdDay, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(15)));

			var ass = AssignmentRepository.GetSingle(thirdDay, agent);
			var dateTimePeriod = new DateTimePeriod(ass.Period.StartDateTime, ass.Period.StartDateTime.AddMinutes(15));
			ass.AddActivity(otherActivity, dateTimePeriod);
			ass.AddPersonalActivity(personalActivity,dateTimePeriod);

			FillSchedulerStateHolder.Fill(SchedulerStateHolder(), null, null, null, period);
			var scheduleDay = SchedulerStateHolder().Schedules[agent].ScheduledDay(thirdDay);
			var scheduleDays = new List<IScheduleDay> { scheduleDay };

			Target.Execute(new NoSchedulingProgress(), scheduleDays, SchedulerStateHolder().SchedulingResultState, SchedulerStateHolder().AllPermittedPersons);

			SchedulerStateHolder().Schedules[agent].ScheduledDay(thirdDay).
				PersonAssignment(true).
				ShiftLayers.Single(x => x.Payload.Equals(otherActivity)).Period.StartDateTime.Minute.
				Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldFindLegalShiftWhenNotAllowedMeetingOverwriteActivity()
		{
			var team = new Team().WithDescription(new Description("team"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var firstDay = new DateOnly(2015, 10, 12);
			var secondDay = firstDay.AddDays(1);
			var thirdDay = firstDay.AddDays(2);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_") {AllowOverwrite = true};
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var meetingActivity = new Activity("personal") {RequiresSkill = false};
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(0, 15, 0, 15, 15), new TimePeriodWithSegment(0, 0, 0, 15, 15)));
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			var skillDays = SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);

			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, secondDay);
			AssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, thirdDay, new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(15)));

			var ass1 = AssignmentRepository.GetSingle(firstDay, agent);
			var ass2 = AssignmentRepository.GetSingle(secondDay, agent);
			var ass3 = AssignmentRepository.GetSingle(thirdDay, agent);
			var dateTimePeriod = new DateTimePeriod(ass3.Period.StartDateTime, ass3.Period.StartDateTime.AddMinutes(15));
			ass3.AddActivity(otherActivity, dateTimePeriod);
			
			var meetingPerson = new MeetingPerson(agent, false);
			var meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", meetingActivity, scenario);
			meeting.SetId(Guid.NewGuid());
			var personMeeting = new PersonMeeting(meeting, meetingPerson, dateTimePeriod);
			personMeeting.SetId(Guid.NewGuid());

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, new IScheduleData[] {ass1, ass2, ass3, personMeeting }, skillDays);
			stateHolder.SchedulingResultState.AllPersonAccounts = new ConcurrentDictionary<IPerson, IPersonAccountCollection>();
			var skillDayDic = new Dictionary<ISkill, IEnumerable<ISkillDay>> {{skill, skillDays}};
			stateHolder.SchedulingResultState.SkillDays = skillDayDic;

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2));
			var scheduleDays = new List<IScheduleDay> {scheduleDay};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, stateHolder.SchedulingResultState, SchedulerStateHolder().AllPermittedPersons);
	
			stateHolder.Schedules[agent].ScheduledDay(thirdDay).
				PersonAssignment(true).
				ShiftLayers.Single(x => x.Payload.Equals(otherActivity)).Period.StartDateTime.Minute.
				Should().Be.EqualTo(15);
		}
	}
}