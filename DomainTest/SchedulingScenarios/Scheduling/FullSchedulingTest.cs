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
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
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
		public ISchedulerStateHolder StateHolder;


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

			Target.DoScheduling(period);

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

			Target.DoScheduling(period, new[] {agent.Id.Value});

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
			StateHolder.SchedulingResultState.UseValidation = true;

			Target.DoScheduling(period);

			AssignmentRepository.Find(new[] { agent }, period, scenario)
				.Count.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldUseMultisiteSkills()
		{
			/* Robin - how should the multisiteskill API be used?
			 * (letters below)
			 * A = Should multisiteskill have explicitly set color, interval length and or skilltype? If so, remove it from ChildSkill? - True
			 * B = Should childskill have explicitly set color, interval length and or skilltype? If so, remove it from MultisiteSkill? - False
			 * C = Should multisiteskill have activity explicitly set? If so, remove it from childskill? - True
			 * D = Should childskill have activity explicitly set? If so, remove it from multisiteskill? - False
			 * E = Agents know childskills and not multisiteskills - right? If so, don't allow multisiteskills as person skills? - True
			 * F = Forecasts are created for multisiteskills and not child skills - right? If so, don't allow forecasts based on childskills? <- A bit more complicated
			 * G = Are workloads belonging to multisiteskills and not child skills? If so, don't allow workloads on childskills? - True
			 */

			DayOffTemplateRepository.Add(new DayOffTemplate());
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var multisiteSkill = new MultisiteSkill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)).WithId(); // A
			WorkloadFactory.CreateWorkloadWithFullOpenHours(multisiteSkill); //G
			multisiteSkill.Activity = activity; //C
			SkillRepository.Has(multisiteSkill);
			var childSkill =  new ChildSkill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)).WithId(); //B
			childSkill.SetParentSkill(multisiteSkill);
			childSkill.Activity = activity; //D
			multisiteSkill.AddChildSkill(childSkill);
			var scenario = ScenarioRepository.Has("some name");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			PersonRepository.Has(new ContractWithMaximumTolerance(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, childSkill); //E
			SkillDayRepository.Has(multisiteSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1)); //F
			SkillDayRepository.Has(childSkill.CreateChildSkillDays(scenario, firstDay, 7));
			MultisiteDayRepository.Has(multisiteSkill.CreateMultisiteDays(scenario,firstDay,7));
			
			Target.DoScheduling(DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1));

			AssignmentRepository.LoadAll().Count(x => x.MainActivities().Any())
				.Should().Be.GreaterThanOrEqualTo(5);
		}

		public FullSchedulingTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}