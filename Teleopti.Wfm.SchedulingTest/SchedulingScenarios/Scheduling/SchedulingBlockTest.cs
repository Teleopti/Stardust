using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingBlockTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleMasterActivityBetweenExtenderActivities(bool useMasterActivity)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var extenderActivity = ActivityRepository.Has("_");
			extenderActivity.RequiresSkill = false;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var activityToUse = useMasterActivity ? masterActivity : activity;
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityToUse, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeEndExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 1, 0, 15)));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}
		
		
		[Test]
		public void ShouldHandleMasterActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(),  new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);	
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}
		
		[Test]
		public void ShouldHandleMasterActivityOnBaseActivityAgentKnowOneOfThePossibleUnderlyingActivities()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var activityAgentDontKnow = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activity);
			masterActivity.ActivityCollection.Add(activityAgentDontKnow);
			var skill = SkillRepository.Has("_", activity);
			var skillAgentDontKnow = SkillRepository.Has("_", activityAgentDontKnow);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillAgentDontKnow.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});

			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario)
				.Where(personAssignment => !personAssignment.AssignedWithDayOff(dayOffTemplate))
				.Count(personAssignment => personAssignment.MainActivities().First().Payload.Equals(activity))
				.Should().Be.EqualTo(5);	
		}
		
		[Test]
		public void ShouldHandleMasterActivityOnExtendedActivityAgentKnowOneOfThePossibleUnderlyingActivities()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var activityExtended = ActivityRepository.Has("A");
			var activityExtendedAgentDontKnow = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(activityExtended);
			masterActivity.ActivityCollection.Add(activityExtendedAgentDontKnow);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", activityExtended);
			var skillExtendedActivityAgentDontKnow = SkillRepository.Has("_", activityExtendedAgentDontKnow);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivityAgentDontKnow.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario)
				.Where(personAssignment => !personAssignment.AssignedWithDayOff(dayOffTemplate))
				.Count(personAssignment => !personAssignment.ShiftLayers.All(x => x.Payload.Equals(activityExtendedAgentDontKnow)) 
											&& personAssignment.ShiftLayers.Any(y => y.Payload.Equals(activityExtended)))
				.Should().Be.EqualTo(5);
		}
		
		[TestCase(true)] //not included because agent doesn't know skill
		[TestCase(false)] //not included because filtered in ShiftFromMasterActivityService (not sure it's correct but that's current impl)
		public void ShouldHandleMasterActivityAsBaseActivity_RequiresSkill_AgentDoesntKnowActivity(bool masterActivityActivityRequiresSkill)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var masterActivityActivity = ActivityRepository.Has("_");
			masterActivityActivity.RequiresSkill = masterActivityActivityRequiresSkill;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(masterActivityActivity);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Any(personAssignment => personAssignment.MainActivities().Any())
				.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMasterActivityOnExtendedActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var otherActivity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(otherActivity);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", otherActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}
		
		[TestCase(true)] //not included because agent doesn't know skill
		[TestCase(false)] //not included because filtered in ShiftFromMasterActivityService (not sure it's correct but that's current impl)
		public void ShouldHandleMasterActivityAsExtendedActivity_RequiresSkill_AgentDoesntKnowActivity(bool masterActivityActivityRequiresSkill)
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var masterActivityActivity = ActivityRepository.Has("_");
			masterActivityActivity.RequiresSkill = masterActivityActivityRequiresSkill;
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(masterActivityActivity);
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Any(personAssignment => personAssignment.MainActivities().Any())
				.Should().Be.False();
		}
		
		[Test]
		public void ShouldGetCorrectLengthOnMasterActivityLayersAsNonBaseActivity()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			var otherActivity = ActivityRepository.Has("_");
			var masterActivity = new MasterActivity().WithId();
			masterActivity.ActivityCollection.Add(otherActivity);
			var skillMainActivity = SkillRepository.Has("_", activity);
			var skillExtendedActivity = SkillRepository.Has("_", otherActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skillMainActivity, skillExtendedActivity);
			SkillDayRepository.Has(skillMainActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			SkillDayRepository.Has(skillExtendedActivity.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate,SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameShiftCategory = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			
			foreach (var personAssignment in PersonAssignmentRepository.Find(new[] {agent}, period, scenario))
			{
				if (personAssignment.DayOff() != null) continue;
				foreach (var shiftLayer in personAssignment.ShiftLayers)
				{
					if (shiftLayer.Payload.Equals(otherActivity))
					{
						shiftLayer.Period.ElapsedTime().Hours.Should().Be.EqualTo(1);
					}
				}
			}
		}
		
		[Test]
		public void ShouldBePossibleToHaveSkillAgentDoesntKnowIfNotRequiresSkillAndNotMasterSkill()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = ActivityRepository.Has("_");
			activity.RequiresSkill = false;
			var skill = SkillRepository.Has("_", activity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, new Skill("_").For(new Activity()).WithId());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameStartTime = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldBePossibleToHaveSkillInExtenderAgentDoesntKnowIfNotRequiresSkillAndNotMasterSkill()
		{
			var firstDay = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var baseActivity = ActivityRepository.Has("_");
			var extenderActivity = ActivityRepository.Has("_");
			extenderActivity.RequiresSkill = false;
			var skill = SkillRepository.Has("_", baseActivity);
			var scenario = ScenarioRepository.Has("_");
			var contractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(baseActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(20, 0, 20, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityRelativeStartExtender(extenderActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(0, 0, 0, 0, 15)));
			var agent = PersonRepository.Has(new Contract("_"), contractSchedule, new PartTimePercentage("_"), new Team(), new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			var dayOffTemplate = new DayOffTemplate(new Description("_")).WithId();
			DayOffTemplateRepository.Add(dayOffTemplate);
			var planningPeriod = PlanningPeriodRepository.Has(period.StartDate, SchedulePeriodType.Week, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.BetweenDayOff;
				x.BlockSameStartTime = true;
			});
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent }, period, scenario).Count(personAssignment => personAssignment.MainActivities().Any()).Should().Be.EqualTo(5);
		}
		
		[Test]
		public void ShouldHandlePartlyOpenedSkillCorrectlyForAgentsInStrangeTimezones()
		{
			DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")).WithId());
			var date = new DateOnly(2015, 10, 12);
			var activity = ActivityRepository.Has("_");
			//opens UTC 10:00
			var skill = SkillRepository.Has("Open", activity, new TimePeriod(11, 19)).InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var scenario = ScenarioRepository.Has("_");
			//starts UTC 10:00
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(3, 0, 3, 0, 15), new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_").WithId()));
			var agent = PersonRepository.Has(new ContractWithMaximumTolerance(), new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, skill).InTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(-1), 1, 1, 1));
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			planningPeriod.PlanningGroup.ModifyDefault(x =>
			{
				x.BlockFinderType = BlockFinderType.SchedulePeriod;
				x.BlockSameShift = true;
			});		
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			PersonAssignmentRepository.Find(new[] { agent}, date.ToDateOnlyPeriod(), scenario).Any(x => x.ShiftLayers.Any()).Should().Be.True();
		}
		

		
		public SchedulingBlockTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}