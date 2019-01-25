using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.UnitTests
{
	[DomainTest]
	[UseIocForFatClient]
	public class BackToLegalShiftCommandTest : SchedulingScenario
	{
		public BackToLegalShiftCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldRestoreToLegalShiftBagShiftWithSameStartAndEndTime()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_");
			var otherActivity = new Activity("other") {RequiresSkill = false};
			var skill = new Skill().For(phoneActivity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1);

			var dayOffTemplate = new DayOffTemplate();
			var assesList = new List<IPersonAssignment> {new PersonAssignment(agent, scenario, firstDay)};
			assesList[0].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(1)));
			assesList[1].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(2)).ShiftCategory(shiftCategory)
				.WithLayer(phoneActivity, new TimePeriod(8, 16)));

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[]{agent}, assesList, skillDays);
			var scheduleDays = new List<IScheduleDay>
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2))
			};
			Target.Execute(new NoSchedulingProgress(), scheduleDays, stateHolder.SchedulingResultState, stateHolder.SchedulingResultState.LoadedAgents);

			var newAssignment = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).PersonAssignment();
			newAssignment.ShiftLayers.Count().Should().Be.EqualTo(2);
			newAssignment.ShiftLayers.First()
				.Period.StartDateTime.Should()
				.Be.EqualTo(scheduleDays.First().PersonAssignment().ShiftLayers.First().Period.StartDateTime);
		}

		[Test]
		public void ShouldKeepOvertimeWhenRestoreToLegalShiftBagShiftWithSameStartAndEndTime()
		{		
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_");
			var otherActivity = new Activity("other") { RequiresSkill = false };
			var skill = new Skill().For(phoneActivity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			agent.Period(firstDay).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(definitionSet);

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1);

			var dayOffTemplate = new DayOffTemplate();
			var assesList = new List<IPersonAssignment> { new PersonAssignment(agent, scenario, firstDay) };
			assesList[0].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(1)));
			assesList[1].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(2)).ShiftCategory(shiftCategory)
				.WithLayer(phoneActivity, new TimePeriod(8, 16)));
			var dateTimePeriod = new DateTimePeriod(assesList[2].Period.StartDateTime.AddHours(-1), assesList[2].Period.StartDateTime);
			assesList[2].AddOvertimeActivity(phoneActivity, dateTimePeriod, definitionSet);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, assesList, skillDays);
			var scheduleDays = new List<IScheduleDay>
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2))
			};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, stateHolder.SchedulingResultState, stateHolder.SchedulingResultState.LoadedAgents);

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).PersonAssignment(true).OvertimeActivities()
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldFindLegalShiftWhenNotAllowPersonalActivityOverwriteActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var otherActivity = new Activity("other") { RequiresSkill = false };
			var personalActivity = new Activity("personal") { RequiresSkill = false };
			var skill = new Skill().For(phoneActivity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(0, 15, 0, 15, 15), new TimePeriodWithSegment(0, 0, 0, 15, 15)));
			var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1);

			var dayOffTemplate = new DayOffTemplate();
			var assesList = new List<IPersonAssignment> { new PersonAssignment(agent, scenario, firstDay) };
			assesList[0].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(1)));
			assesList[1].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(2)).ShiftCategory(shiftCategory)
				.WithLayer(phoneActivity, new TimePeriod(7, 15)));
			var dateTimePeriod = new DateTimePeriod(assesList[2].Period.StartDateTime, assesList[2].Period.StartDateTime.AddMinutes(15));
			assesList[2].AddActivity(otherActivity, dateTimePeriod);
			assesList[2].AddPersonalActivity(personalActivity, dateTimePeriod);

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, assesList, skillDays);
			var scheduleDays = new List<IScheduleDay>
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2))
			};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, stateHolder.SchedulingResultState, stateHolder.SchedulingResultState.LoadedAgents);

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).
				PersonAssignment(true).
				ShiftLayers.Single(x => x.Payload.Equals(otherActivity)).Period.StartDateTime.Minute.
				Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldFindLegalShiftWhenNotAllowedMeetingOverwriteActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var otherActivity = new Activity("other") { RequiresSkill = false };
			var meetingActivity = new Activity("personal") { RequiresSkill = false };
			var skill = new Skill().For(phoneActivity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(0, 15, 0, 15, 15), new TimePeriodWithSegment(0, 0, 0, 15, 15)));
			var agent = new Person().WithId().WithPersonPeriod(ruleSet, skill);
			agent.AddSchedulePeriod(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1));
			
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1);

			var dayOffTemplate = new DayOffTemplate();
			var assesList = new List<IPersonAssignment> { new PersonAssignment(agent, scenario, firstDay) };
			assesList[0].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(1)));
			assesList[1].SetDayOff(dayOffTemplate);
			assesList.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(2)).ShiftCategory(shiftCategory)
				.WithLayer(phoneActivity, new TimePeriod(7, 15)));
			var dateTimePeriod = new DateTimePeriod(assesList[2].Period.StartDateTime, assesList[2].Period.StartDateTime.AddMinutes(15));
			assesList[2].AddActivity(otherActivity, dateTimePeriod);

			var meetingPerson = new MeetingPerson(agent, false);
			var meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", meetingActivity, scenario);
			meeting.SetId(Guid.NewGuid());
			var personMeeting = new PersonMeeting(meeting, meetingPerson, dateTimePeriod);

			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, new IScheduleData[] { assesList[0], assesList[1], assesList[2], personMeeting }, skillDays);
			var scheduleDays = new List<IScheduleDay>
			{
				stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2))
			};

			Target.Execute(new NoSchedulingProgress(), scheduleDays, stateHolder.SchedulingResultState, stateHolder.SchedulingResultState.LoadedAgents);

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).
				PersonAssignment(true).
				ShiftLayers.Single(x => x.Payload.Equals(otherActivity)).Period.StartDateTime.Minute.
				Should().Be.EqualTo(15);
		}

		public BackToLegalShiftCommandTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}