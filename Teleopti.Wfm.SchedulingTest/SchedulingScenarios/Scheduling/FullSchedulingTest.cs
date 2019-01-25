using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class FullSchedulingTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeMultisiteDayRepository MultisiteDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeStudentAvailabilityDayRepository StudentAvailabilityDayRepository;
		public FakeSkillCombinationResourceReader SkillCombinationResourceReader;
		
		[Test]
		public void ShouldNotCreateTags()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Day, 8);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] {agent}, period, scenario).Should().Not.Be.Empty();
			AgentDayScheduleTagRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotCreateTagsForPeople()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
				);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Day, 8);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Should().Not.Be.Empty();
			AgentDayScheduleTagRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldResolveNightlyRest()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity, new TimePeriod(8, 16));
			var workloadDayTemplateMonday = skill.WorkloadCollection.Single().TemplateWeekCollection.Single(x => x.Value.DayOfWeek == DayOfWeek.Monday);
			workloadDayTemplateMonday.Value.ChangeOpenHours(new [] { new TimePeriod(0, 24) });
			var scenario = ScenarioRepository.Has("_");
			var contract = new ContractWithMaximumTolerance{WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(11), TimeSpan.FromHours(1))};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSetLate = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(15, 0, 15, 0, 15), new TimePeriodWithSegment(23, 0, 23, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(ruleSet, ruleSetLate);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var lateInterval = new TimePeriod(19, 45, 20, 0);
			var earlyInterval = new TimePeriod(9, 45, 10, 0);
			SkillDayRepository.Has(
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(0), 1, new Tuple<TimePeriod, double>(lateInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(1), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(2), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(3), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(4), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(5), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)), 
				skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(6), 1, new Tuple<TimePeriod, double>(earlyInterval, 1000)));
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario)
				.Count().Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldUseMultisiteSkills()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate());
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var multisiteSkill = new MultisiteSkill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(multisiteSkill);
			SkillRepository.Has(multisiteSkill);
			var childSkill = new ChildSkill("_", "_", Color.Empty, multisiteSkill).WithId();
			SkillRepository.Has(childSkill);
			multisiteSkill.AddChildSkill(childSkill);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			PersonRepository.Has(new ContractWithMaximumTolerance(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, childSkill);
			SkillDayRepository.Has(multisiteSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(childSkill.CreateChildSkillDays(scenario, firstDay, 7));
			MultisiteDayRepository.Has(multisiteSkill.CreateMultisiteDays(scenario,firstDay,7));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Week, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll().Count(x => x.MainActivities().Any())
				.Should().Be.GreaterThanOrEqualTo(5);
		}

		[Test]
		public void ShouldRespectTargetDayOffs()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contractAcceptingEverythingButDayOffBreaking = new ContractWithMaximumTolerance { NegativeDayOffTolerance = 0,  PositiveDayOffTolerance = 0};
			var agent = PersonRepository.Has(contractAcceptingEverythingButDayOffBreaking, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			for (var day = 0; day < 3; day++)
			{
				AssignmentRepository.Add(new PersonAssignment(agent, scenario, firstDay.AddDays(day)).WithDayOff());
			}
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Any(x => x.MainActivities().Any())
				.Should().Be.False();
		}

		[Test]
		public void ShouldRespectAccessibility([Values(true, false)]bool blockedByAccessibility)
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var monday = new DateOnly(2017, 8, 21);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet8 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSet10 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory));
			var bag = new RuleSetBag(ruleSet8, ruleSet10);
			ruleSet8.AddAccessibilityDayOfWeek(blockedByAccessibility ? DayOfWeek.Monday : DayOfWeek.Tuesday);
			var agent = PersonRepository.Has(new SchedulePeriod(monday, SchedulePeriodType.Day, 1), bag, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(scenario, monday, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), 2)));
			var planningPeriod = PlanningPeriodRepository.Has(monday,SchedulePeriodType.Day, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(monday, agent).Period.StartDateTime.Hour
				.Should().Be.EqualTo(blockedByAccessibility ? 10 : 8);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(20)]
		public void ShouldNotCareAboutServiceLevelSecondsIfManualAgentsIsSet(int serviceLevelInSeconds)
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2017, 8, 21);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet8 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSet10 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory));
			var bag = new RuleSetBag(ruleSet8, ruleSet10);
			var agent = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), bag, skill);
			const int manualAgentsDefault = 1;
			const int manualAgentsAt8OClock = 2;
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, date, manualAgentsDefault, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), manualAgentsAt8OClock));
			foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
			{
				if (skillDataPeriod.Period.StartDateTime.Hour == 8)
				{
					skillDataPeriod.ServiceLevelSeconds = serviceLevelInSeconds;
				}
			}
			SkillDayRepository.Has(skillDay);
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.GetSingle(date, agent).Period.StartDateTime.Hour
				.Should().Be.EqualTo(8);
		}
		
		[TestCase(0, ExpectedResult = 8)]
		[TestCase(0.1, ExpectedResult = 8)]
		[TestCase(1.1, ExpectedResult = 10)]
		public int ShouldConsiderExternalStaff(double externalResources)
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var date = new DateOnly(2017, 8, 21);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity).DefaultResolution(60);
			var scenario = ScenarioRepository.Has("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet8 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSet10 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), shiftCategory));
			var bag = new RuleSetBag(ruleSet8, ruleSet10);
			var agentToSchedule = PersonRepository.Has(new SchedulePeriod(date, SchedulePeriodType.Day, 1), bag, skill);
			var alreadyScheduledAgent = PersonRepository.Has(skill);
			AssignmentRepository.Has(alreadyScheduledAgent, scenario, activity, shiftCategory, date, new TimePeriod(8, 18));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), 2)));
			SkillCombinationResourceReader.Has(externalResources, new DateTimePeriod(new DateTime(date.Date.AddHours(8).Ticks, DateTimeKind.Utc), new DateTime(date.Date.AddHours(9).Ticks, DateTimeKind.Utc)), skill);
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			return AssignmentRepository.GetSingle(date, agentToSchedule).Period.StartDateTime.Hour;
		}

		public FullSchedulingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}