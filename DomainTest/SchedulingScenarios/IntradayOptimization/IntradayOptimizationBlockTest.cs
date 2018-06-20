using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DomainTest]
	public class IntradayOptimizationBlockTest : IntradayOptimizationScenarioTest
	{
		public IntradayOptimizationFromWeb Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesDefaultValueProvider;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;

		[Test]
		public void ShouldIndividualFlexableWhenNotBlock()
		{
			var dateOnly = new DateOnly(2017, 9, 25);

			var activity = ActivityFactory.CreateActivity("phone");
			var skill1 = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(60), TimeSpan.FromHours(11), TimeSpan.FromHours(8));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(0), NegativeDayOffTolerance = 3 };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			var agent1 = PersonRepository.Has(contract, ContractScheduleFactory.Create7DaysWorkingContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill1);

			for (int i = 0; i < 7; i++)
			{
				SkillDayRepository.Has(
					i == 6
						? skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(9, TimeSpan.FromMinutes(180)))
						: skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(180)))
				);

				var ass = new PersonAssignment(agent1, scenario, dateOnly.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(9, 0, 17, 0)).WithId();
				PersonAssignmentRepository.Has(ass);
			}

			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);


			Target.Execute(planningPeriod.Id.Value);

			for (int i = 0; i < 7; i++)
			{
				var date = dateOnly.AddDays(i);
				var dateTime1 = TimeZoneHelper.ConvertToUtc(date.Date, agent1.PermissionInformation.DefaultTimeZone());
				PersonAssignmentRepository.GetSingle(date, agent1).Period
					.Should()
					.Be.EqualTo(i == 6
						? new DateTimePeriod(dateTime1.AddHours(9), dateTime1.AddHours(17))
						: new DateTimePeriod(dateTime1.AddHours(10), dateTime1.AddHours(18)));
			}
		}

		[Test]
		public void ShouldUseSameShiftWhenBlock()
		{
			var dateOnly = new DateOnly(2017, 9, 25);

			var activity = ActivityFactory.CreateActivity("phone");
			var skill1 = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(60), TimeSpan.FromHours(11), TimeSpan.FromHours(8));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(0), NegativeDayOffTolerance = 3 };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			var agent1 = PersonRepository.Has(contract, ContractScheduleFactory.Create7DaysWorkingContractSchedule(), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill1);
			
			for (int i = 0; i < 7; i++)
			{
				SkillDayRepository.Has(
					i == 6
						? skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(9, TimeSpan.FromMinutes(180)))
						: skill1.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(i), TimeSpan.FromMinutes(60),
							new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(180)))
				);

				var ass = new PersonAssignment(agent1, scenario, dateOnly.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(9, 0, 17, 0)).WithId();
				PersonAssignmentRepository.Has(ass);
			}

			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);


			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShift = true, BlockTypeValue = BlockFinderType.SchedulePeriod }
			};
			OptimizationPreferencesDefaultValueProvider.SetFromTestsOnly(optimizationPreferences);

			Target.Execute(planningPeriod.Id.Value);

			for (int i = 0; i < 7; i++)
			{
				var date = dateOnly.AddDays(i);
				var dateTime1 = TimeZoneHelper.ConvertToUtc(date.Date, agent1.PermissionInformation.DefaultTimeZone());
				PersonAssignmentRepository.GetSingle(date, agent1).Period
					.Should().Be.EqualTo(new DateTimePeriod(dateTime1.AddHours(10), dateTime1.AddHours(18)));
			}
		}

		[Test]
		[Ignore("to be fixed")]
		public void ShouldHandleAgentsWithDifferentSameStartTime()
		{
			var date = new DateOnly(2017, 9, 25);
			var activity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 15), new TimePeriodWithSegment(17, 0, 18, 0, 15), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 10));
			var agentWithSameStarttime = PersonRepository.Has(ruleSet, skill);
			var agentWithNotSameStarttime = PersonRepository.Has(ruleSet, skill);
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithSameStarttime, scenario, date.AddDays(0)).WithDayOff());
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithNotSameStarttime, scenario, date.AddDays(0)).WithDayOff());
			Enumerable.Range(1, 5).ForEach(x =>
			{
				PersonAssignmentRepository.Has(agentWithSameStarttime, scenario, activity, shiftCategory, date.AddDays(x), new TimePeriod(8, 17));
				PersonAssignmentRepository.Has(agentWithNotSameStarttime, scenario, activity, shiftCategory, date.AddDays(x), new TimePeriod(8, 17));
			});
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithSameStarttime, scenario, date.AddDays(6)).WithDayOff());
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithNotSameStarttime, scenario, date.AddDays(6)).WithDayOff());
			var planningPeriod = PlanningPeriodRepository.Has(date, 1);
			var planningGroupSettings = PlanningGroupSettings.CreateDefault(planningPeriod.PlanningGroup);
			planningGroupSettings.AddFilter(new TeamFilter(agentWithSameStarttime.MyTeam(date)));
			planningGroupSettings.BlockFinderType = BlockFinderType.BetweenDayOff;
			planningGroupSettings.BlockSameStartTime = true;
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			
			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.LoadAll().Where(x => x.Person.Equals(agentWithSameStarttime) && x.DayOff()==null)
				.Select(x => x.Period.StartDateTime.TimeOfDay).Distinct().Count()
				.Should().Be.EqualTo(1);
			PersonAssignmentRepository.LoadAll().Where(x => x.Person.Equals(agentWithNotSameStarttime) && x.DayOff()==null)
				.Select(x => x.Period.StartDateTime.TimeOfDay).Distinct().Count()
				.Should().Be.GreaterThan(1);
		}
	}
}