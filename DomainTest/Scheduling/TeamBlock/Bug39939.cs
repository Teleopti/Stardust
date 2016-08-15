using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[DomainTest]
	public class Bug39939
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		public void ShouldHandleStrangeSchedules()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var team = new Team { Description = new Description("team") };
			//remove this - must currently be here due to GroupPageCreator reads businessunit.sitecollection, fix that!
			var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var site = new Site("site");
			site.AddTeam(team);
			businessUnit.AddSite(site);
			//
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill).InTimeZone(TimeZoneInfoFactory.TaipeiTimeZoneInfo());
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill).InTimeZone(TimeZoneInfoFactory.TaipeiTimeZoneInfo());
			var shiftCategory = new ShiftCategory("_").WithId();
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnlyPeriod(firstDay, firstDay.AddDays(6)), TimeSpan.FromHours(10)));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay, new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay.AddDays(1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay.AddDays(2), new TimePeriod(TimeSpan.FromHours(20), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6))));
			PersonAssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(4));
			PersonAssignmentRepository.Has(agent1, scenario, activity, shiftCategory, firstDay.AddDays(6), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent1, scenario, dayOffTemplate, firstDay.AddDays(7));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay, new TimePeriod(0, 0, 8, 0));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay.AddDays(1), new TimePeriod(0, 0, 8, 0));
			PersonAssignmentRepository.Has(agent2, scenario, dayOffTemplate, firstDay.AddDays(2));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay.AddDays(4), new TimePeriod(TimeSpan.FromHours(3), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(11))));
			PersonAssignmentRepository.Has(agent2, scenario, activity, shiftCategory, firstDay.AddDays(5), new TimePeriod(0, 0, 8, 0));
			PersonAssignmentRepository.Has(agent2, scenario, dayOffTemplate, firstDay.AddDays(6));
			PersonAssignmentRepository.Has(agent2, scenario, dayOffTemplate, firstDay.AddDays(7));
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
				UseTeam = true,
				TeamSameShiftCategory = true,
				TagToUseOnScheduling = NullScheduleTag.Instance
			});

			Target.DoScheduling(new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1)));

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(12);
		}
	}
}