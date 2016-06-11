using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class BackToLegalShiftCommandTest
	{
		public BackToLegalShiftCommand Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IFillSchedulerStateHolder FillSchedulerStateHolder;

		[Test]
		public void ShouldRestoreToLegalShiftBagShiftWithSameStartAndEndTime()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var phoneActivity = ActivityRepository.Has("_");
			var otherActivity = ActivityRepository.Has("other");
			otherActivity.RequiresSkill = false;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var scenario = ScenarioRepository.Has("some name");
			var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var site = new Site("site");
			var team = new Team { Description = new Description("team") };
			site.AddTeam(team);
			businessUnit.AddSite(site);
			BusinessUnitRepository.Has(businessUnit);
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(7, 0, 9, 0, 15),
					new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(otherActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15),
				new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var ruleSetBag = new RuleSetBag(ruleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10))
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

			Target.Execute(new NoSchedulingProgress(), scheduleDays, SchedulerStateHolder().SchedulingResultState);

			var newAssignment = SchedulerStateHolder().Schedules[agent1].ScheduledDay(firstDay.AddDays(2)).PersonAssignment();
			newAssignment.ShiftLayers.Count().Should().Be.EqualTo(2);
			newAssignment.ShiftLayers.First()
				.Period.StartDateTime.Should()
				.Be.EqualTo(scheduleDays.First().PersonAssignment().ShiftLayers.First().Period.StartDateTime);
		}
	}
}