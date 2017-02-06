using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[LoggedOnAppDomain]
	[TestFixture(true, true)]
	[TestFixture(true, false)]
	[TestFixture(false, true)]
	[TestFixture(false, false)]
	public class IntradayOptimizationTest : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerSplitBigIslands42049;
		private readonly bool _resourcePlannerIntradayNoDailyValueCheck42767;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationFromWeb Target;
		public IPersonWeekViolatingWeeklyRestSpecification CheckWeeklyRestRule;
		public IScheduleStorage ScheduleStorage;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public Func<IGridlockManager> LockManager;

		public IntradayOptimizationTest(bool resourcePlannerSplitBigIslands42049, bool resourcePlannerIntradayNoDailyValueCheck42767)
		{
			_resourcePlannerSplitBigIslands42049 = resourcePlannerSplitBigIslands42049;
			_resourcePlannerIntradayNoDailyValueCheck42767 = resourcePlannerIntradayNoDailyValueCheck42767;
		}

		[Test]
		public void ShouldUseShiftThatCoverHigherDemand()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract"){WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(15), dateTime.AddHours(17).AddMinutes(15)));
		}

		[Test]
		public void ShouldNotOptimizeUnactiveSkill()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));
			((IPersonSkillModify)agent.Period(dateOnly).PersonSkillCollection.Single()).Active = false;

			Target.Execute(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(0), dateTime.AddHours(17).AddMinutes(0)));
		}

		[Test]
		public void ShouldNotOptimizeDaysWithOvertime()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(9), dateTime.AddHours(17)));
			assignment.AddOvertimeActivity(phoneActivity, new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(9)),
											MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("multiplicator", MultiplicatorType.Overtime));
			PersonAssignmentRepository.Add(assignment);

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
		}

		[Test]
		public void ShouldResolveWeeklyRest()
		{
			var weeklyRest = TimeSpan.FromHours(38);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.InWorkTime = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var weekPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7));
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), weeklyRest);
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));

			Target.Execute(planningPeriod.Id.Value);

			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false, false), weekPeriod, scenario)[agent];
			CheckWeeklyRestRule.IsSatisfyBy(agentRange, weekPeriod, weeklyRest).Should().Be.True();	
		}

		[Test]
		public void ShouldCalculateDayAfterIfTodayIsANightShift()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360))),
				skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
			});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(17, 0, 26, 0));
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly.AddDays(1), new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);
			
			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly.AddDays(1), dateOnly.AddDays(1)), new List<ISkill> { skill }, scenario);
			skillDays.First().SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldCalculateDayBeforeDueToTimeZone()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill).InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			SkillDayRepository.Has(new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), 1), //här hamnar ursprungsassignment
				skill.CreateSkillDayWithDemand(scenario, dateOnly, 1),
				skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 1)
			});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(0, 0, 9, 0)); //ska börja tidigt! för att få rött

			Target.Execute(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(dateOnly.AddDays(-1).ToDateOnlyPeriod(), new List<ISkill> { skill }, scenario);
			skillDays.First().SkillStaffPeriodCollection.Any(x => x.CalculatedResource == 1)
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCalculateIntraIntervalIssuses()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly.AddDays(-6), 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 8, 10));

			Target.Execute(planningPeriod.Id.Value);

			var skillDays = SkillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateOnly, dateOnly), new List<ISkill> { skill }, scenario);
			var skillStaffPeriods = skillDays.First().SkillStaffPeriodCollection;
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				skillStaffPeriod.HasIntraIntervalIssue.Should().Be.False();
			}	
		}

		[Test]
		public void ShouldNotLoopForeverIfSkillDayDoesntExists()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Assert.DoesNotThrow(() =>
				Target.Execute(planningPeriod.Id.Value));
		}

		[Test]
		public void ShouldNotTouchLockedDays()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)))
				});
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));
			LockManager().AddLock(agent, dateOnly, LockType.Normal); //why is period needed?

			Target.Execute(planningPeriod.Id.Value);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agent.PermissionInformation.DefaultTimeZone());
			PersonAssignmentRepository.GetSingle(dateOnly).Period
				.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8), dateTime.AddHours(17)));
		}

		[Test, Ignore("PBI 42767")]
		public void ShouldNotRollBackEvenIfStandardDevGetsHigher()
		{
			//PBI 42767
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			var mailActivity = ActivityFactory.CreateActivity("mail");
			mailActivity.RequiresSkill = true;
			var phoneSkill = SkillRepository.Has("phone", phoneActivity, new TimePeriod(8, 16)).InTimeZone(TimeZoneInfo.Utc);
			phoneSkill.DefaultResolution = 30;

			var mailSkill = SkillRepository.Has("mail", mailActivity, new TimePeriod(0, 24)).InTimeZone(TimeZoneInfo.Utc);
			mailSkill.DefaultResolution = 60;
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract")
			{
				WorkTimeDirective = worktimeDirective,
				EmploymentType = EmploymentType.FixedStaffDayWorkTime,
				WorkTime = new WorkTime(TimeSpan.FromHours(8))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet1 =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			ruleSet1.AddExtender(new ActivityRelativeStartExtender(mailActivity, new TimePeriodWithSegment(4, 0, 4, 0, 15),
				new TimePeriodWithSegment(0, 0, 4, 0, 120)));
			var ruleSet2 =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(mailActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent1 = PersonRepository.Has(contract, schedulePeriod, ruleSet1, phoneSkill, mailSkill);
			agent1.SetName(new Name("1", "1"));
			var agent2 = PersonRepository.Has(contract, schedulePeriod, ruleSet2, phoneSkill, mailSkill);
			agent2.SetName(new Name("2", "2"));

			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					phoneSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(1)),
					mailSkill.CreateEmailSkillDayWithIncomingDemandOncePerDay(scenario,dateOnly,TimeSpan.FromHours(5), new TimePeriod(0,24))
				});

			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("x"), dateOnly, new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent2, scenario, mailActivity, new ShiftCategory("x"), dateOnly, new TimePeriod(8, 0, 16, 0));

			Target.Execute(planningPeriod.Id.Value);

			if(_resourcePlannerIntradayNoDailyValueCheck42767)
				PersonAssignmentRepository.GetSingle(dateOnly, agent1).ShiftCategory.Should().Be.EqualTo(shiftCategory);
			else
				PersonAssignmentRepository.GetSingle(dateOnly, agent1).ShiftCategory.Should().Not.Be.EqualTo(shiftCategory);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerSplitBigIslands42049)
				toggleManager.Enable(Toggles.ResourcePlanner_SplitBigIslands_42049);
			if (_resourcePlannerIntradayNoDailyValueCheck42767)
				toggleManager.Enable(Toggles.ResourcePlanner_IntradayNoDailyValueCheck_42767);
		}
	}
}
