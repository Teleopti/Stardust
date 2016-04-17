using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class FullSchedulingTest
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		//public FakePreferenceDayRepository PreferenceDayRepository;

		[Test]
		public void ShouldNotCreateTags()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10))
				);

			Target.DoScheduling(period);

			AssignmentRepository.Find(new[] {agent}, period, scenario).Should().Not.Be.Empty();
			AgentDayScheduleTagRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotScheduleDaysOffOutsideSelectedDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); 
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(ruleSet);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10),
				TimeSpan.FromHours(10))
				);

			Target.DoScheduling(new DateOnlyPeriod(new DateOnly(2015,10,16), new DateOnly(2015, 10, 17))); //friday saturday

			var assignments = AssignmentRepository.Find(new[] {agent}, period, scenario);
			assignments.Count.Should().Be.EqualTo(1);

			var assignment = assignments.First();
			assignment.Date.Should().Be.EqualTo(new DateOnly(2015, 10, 17));
			assignment.DayOff().Should().Not.Be.Null();
		}

		

		[Test, Ignore("Need help on this")]
		public void TeamBlockSchedulingShouldNotUseShiftsMarkedForRestrictionOnlyWhenThereIsNoRestriction()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
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
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			restrictedRuleSet.OnlyForRestrictions = true;
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
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
			var schedulingOptions = new SchedulingOptions
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
			};

			Target.DoScheduling(period, schedulingOptions);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))
					continue;

				personAssignment.MainActivities()
					.First()
					.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
					.TimeOfDay
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(12)); //early shifts are not allowed
			}
		}

		[Test, Ignore("Need help on this")]
		public void TeamBlockSchedulingShouldNotUseShiftsMarkedForRestrictionOnlyWhenThereIsNoRestrictionOnSingleAgentTeams()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var site = new Site("site");
			var team1 = new Team { Description = new Description("team1") };
			var team2 = new Team { Description = new Description("team2") };
			site.AddTeam(team1);
			site.AddTeam(team2);
			businessUnit.AddSite(site);
			BusinessUnitRepository.Has(businessUnit);
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team1, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team2, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			restrictedRuleSet.OnlyForRestrictions = true;
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
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
			var schedulingOptions = new SchedulingOptions
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
			};

			Target.DoScheduling(period, schedulingOptions);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))
					continue;

				personAssignment.MainActivities()
					.First()
					.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
					.TimeOfDay
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(12)); //early shifts are not allowed
			}
		}

		[Test, Ignore("Need help on this")]
		public void TeamBlockSchedulingShouldUseShiftsMarkedForRestrictionOnlyWhenThereIsRestriction()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6)); //12 to 18
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
			var site = new Site("site");
			var team1 = new Team { Description = new Description("team1") };
			var team2 = new Team { Description = new Description("team2") };
			site.AddTeam(team1);
			site.AddTeam(team2);
			businessUnit.AddSite(site);
			BusinessUnitRepository.Has(businessUnit);
			var contract = new Contract("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var agent1 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team1, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var agent2 = PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), team2, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var shiftCategory = new ShiftCategory("_").WithId();
			var restrictedRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var normalRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			restrictedRuleSet.OnlyForRestrictions = true;
			var ruleSetBag = new RuleSetBag(restrictedRuleSet);
			ruleSetBag.AddRuleSet(normalRuleSet);
			agent1.Period(firstDay).RuleSetBag = ruleSetBag;
			agent2.Period(firstDay).RuleSetBag = ruleSetBag;
			//PreferenceDayRepository.Add(new PreferenceDay(agent2, firstDay,
			//	new PreferenceRestriction
			//	{
			//		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))
			//	}).WithId());
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
			var schedulingOptions = new SchedulingOptions
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
			};

			Target.DoScheduling(period, schedulingOptions);

			var assignments = AssignmentRepository.Find(new[] { agent1, agent2 }, period, scenario);
			assignments.Count.Should().Be.EqualTo(14);

			var foundRestrictedAssignments = 0;
			foreach (var personAssignment in assignments)
			{
				if (personAssignment.AssignedWithDayOff(dayOffTemplate))
					continue;

				if (
					personAssignment.MainActivities()
						.First()
						.Period.StartDateTimeLocal(personAssignment.Person.PermissionInformation.DefaultTimeZone())
						.TimeOfDay.Equals(TimeSpan.FromHours(8)))
					foundRestrictedAssignments++;
			}
			foundRestrictedAssignments.Should().Be.EqualTo(1);
		}
	}
}