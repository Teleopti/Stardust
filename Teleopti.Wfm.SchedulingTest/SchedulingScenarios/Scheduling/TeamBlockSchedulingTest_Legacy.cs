using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	/*  DONT ADD MORE TESTS HERE! - LEGACY TESTS HERE!
	 *  Web supports (limited) block and team scheduling only.
	 *  If you want to add simple plan block scheduling tests, add it to SchedulingBlockTest
	 *  If you want to add simple plan team scheduling tests, add it to TeamBlockSchedulingTest
	 *  If you want to add a mix of these or other cases not supported in plans, add it to a desktop test
	 */
	[DomainTest]
	public class TeamBlockSchedulingTest_Legacy : SchedulingScenario, IIsolateSystem
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePersonRotationRepository PersonRotationRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMixOfTeamAndBlockAndNotClearToMuch_BetweenDayOffs(bool reversedAgentOrder)
		{
			var firstDay = new DateOnly(2016, 05, 30);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var otherShiftCategory = new ShiftCategory("other").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") }.WithId();
			RuleSetBagRepository.Add(ruleSetBag);
			var contract = new ContractWithMaximumTolerance().WithNoDayOffTolerance();
			var agent1 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var agent2 = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			if (reversedAgentOrder)
				PersonRepository.ReversedOrder();
			DayOffTemplateRepository.Add(new DayOffTemplate());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 2, 2, 2, 2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(2));
			AssignmentRepository.Has(agent1, scenario, new DayOffTemplate(), firstDay.AddDays(4));
			AssignmentRepository.Has(agent2, scenario, new DayOffTemplate(), firstDay.AddDays(5));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(4), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay, new TimePeriod(8, 16));
			AssignmentRepository.Has(agent1, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay, new TimePeriod(8, 16));
			AssignmentRepository.Has(agent2, scenario, activity, otherShiftCategory, firstDay.AddDays(1), new TimePeriod(8, 16));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay,SchedulePeriodType.Day, 5);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag),
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				TeamSameShiftCategory = true,
				BlockSameShiftCategory = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			AssignmentRepository.LoadAll()
				.Count(x => otherShiftCategory.Equals(x.ShiftCategory))
				.Should()
				.Be.GreaterThanOrEqualTo(4); //<--
		}
		
		[Test]
		public void ShouldHandleBlockSameStartTimeInCombinationWithRotationWithSpecifiedShiftCategoryOnBlockStartingDateBug41378()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var scenario = ScenarioRepository.Has("_");
			var phoneActivity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("Open", phoneActivity, new TimePeriod(12, 0, 21, 0)).InTimeZone(TimeZoneInfo.Utc);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,10, 10, 10, 10, 10, 10, 10));
			var shiftCategory8H15M = new ShiftCategory("L").WithId();
			var ruleSet8H15M = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 15, 20, 15, 15), shiftCategory8H15M));
			var ruleSetBag = new RuleSetBag(ruleSet8H15M);
			var shiftCategory7H = new ShiftCategory("S").WithId();
			var ruleSet7H = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(19, 0, 19, 0, 15), shiftCategory7H));
			ruleSetBag.AddRuleSet(ruleSet7H);
			var agent = PersonRepository.Has(new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1),ruleSetBag, skill);
			var schedulePeriod = agent.SchedulePeriod(firstDay);
			schedulePeriod.SetDaysOff(2);
			var dayOffTemplate = new DayOffTemplate(new Description("DO", "DO"));
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate, firstDay.AddDays(6));
			var rotation = new Rotation("_", 7);
			rotation.RotationDays[1].RestrictionCollection[0].ShiftCategory = shiftCategory7H;
			var personRotation = new PersonRotation(agent, rotation, firstDay, 0).WithId();
			PersonRotationRepository.Add(personRotation);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				UseRotations = true,
				UseBlock = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameStartTime = true,
				BlockSameShiftCategory = false,
				UseAverageShiftLengths = true
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value, false);
			
			AssignmentRepository.Find(new[] { agent}, firstDay.ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			AssignmentRepository.Find(new[] {agent}, firstDay.AddDays(1).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory7H);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(2).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(3).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(4).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(5).ToDateOnlyPeriod(), scenario).Single().ShiftCategory.Should().Be.EqualTo(shiftCategory8H15M);
			AssignmentRepository.Find(new[] { agent}, firstDay.AddDays(6).ToDateOnlyPeriod(), scenario).Single().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}
		
		[Test]
		public void ShouldBePossibleToScheduleBlockBetweenDaysOffSameShiftWhenSkillIsClosedOneDay()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())));
			foreach (var dayTemplate in skill.WorkloadCollection.First().TemplateWeekCollection.Values)
			{
				if (dayTemplate.DayOfWeek == DayOfWeek.Monday || dayTemplate.DayOfWeek == DayOfWeek.Sunday || dayTemplate.DayOfWeek == DayOfWeek.Saturday)
				{
					dayTemplate.Close();
				}
			}
			PersonAbsenceRepository.Has(new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence { InContractTime = true },
				firstDay.ToDateTimePeriod(new TimePeriod(8, 16), TimeZoneInfo.Utc))));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0, 1, 1, 1, 1, 0, 0));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			AssignmentRepository.Has(agent, scenario, dayOffTemplate,firstDay.AddDays(-1));			
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				UseAverageShiftLengths = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count(x => x.MainActivities().Any() || x.AssignedWithDayOff(dayOffTemplate)).Should().Be.EqualTo(6);
		}
		
		[Test]
		public void ShouldBePossibleToScheduleBlockSchedulePeriodSameShiftWhenSkillIsClosedOneDay()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var agent = PersonRepository.Has(new Contract("_"), ContractScheduleFactory.CreateWorkingWeekContractSchedule(),
				new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			agent.Period(firstDay).RuleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())));

			foreach (var dayTemplate in skill.WorkloadCollection.First().TemplateWeekCollection.Values)
			{
				if (dayTemplate.DayOfWeek == DayOfWeek.Monday || dayTemplate.DayOfWeek == DayOfWeek.Sunday || dayTemplate.DayOfWeek == DayOfWeek.Saturday)
				{
					dayTemplate.Close();
				}
			}
			PersonAbsenceRepository.Has(new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence { InContractTime = true },
				firstDay.ToDateTimePeriod(new TimePeriod(8, 16), TimeZoneInfo.Utc))));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0, 1, 1, 1, 1, 0, 0));
			var dayOffTemplate = new DayOffTemplate(new Description("_default")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			SchedulingOptionsProvider.SetFromTest_LegacyDONOTUSE(planningPeriod, new SchedulingOptions
			{
				DayOffTemplate = dayOffTemplate,
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				UseBlock = true,
				BlockSameShift = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod,
				UseAverageShiftLengths = true
			});

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);


			var assignments = AssignmentRepository.Find(new[] { agent }, period, scenario);
			assignments.Count(x => x.MainActivities().Any() || x.AssignedWithDayOff(dayOffTemplate)).Should().Be.EqualTo(6);
		}
		
		public void Isolate(IIsolate isolate)
		{
			//hack until web supports team scheduling
			isolate.UseTestDouble<SchedulingOptionsProvider>().For<ISchedulingOptionsProvider>();
		}

		public TeamBlockSchedulingTest_Legacy(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}