using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
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
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
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
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
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
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
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
		[Ignore("44622 - to be fixed")]
		public void ShouldResolveNightlyRest()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(6));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			foreach (var workloadDayTemplate in skill.WorkloadCollection.Single().TemplateWeekCollection)
			{
				if (!workloadDayTemplate.Value.DayOfWeek.Equals(DayOfWeek.Monday))
				{
					workloadDayTemplate.Value.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(8, 16) });
				}
			}
			var scenario = ScenarioRepository.Has("_");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());	
			var contract = new ContractWithMaximumTolerance{WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(11), TimeSpan.FromHours(1))};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSetLate = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(15, 0, 15, 0, 15), new TimePeriodWithSegment(23, 0, 23, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(ruleSet, ruleSetLate);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSetBag, skill);
			var lateInterval = new TimePeriod(19, 45, 20, 0);
			var earlyInterval = new TimePeriod(9, 45, 10, 0);
			var skillDayMonday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay, 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(lateInterval, 1000));
			var skillDayTuesday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(1), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			var skillDayWednesday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(2), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			var skillDayThursday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(3), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			var skillDayFriday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(4), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			var skillDaySaturday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(5), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			var skillDaySunday = skill.CreateSkillDayWithDemandOnInterval(scenario, firstDay.AddDays(6), 1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(earlyInterval, 1000));
			SkillDayRepository.Has(skillDayMonday, skillDayTuesday, skillDayWednesday, skillDayThursday, skillDayFriday, skillDaySaturday,skillDaySunday);
			StateHolder.SchedulingResultState.UseValidation = true;
			Target.DoScheduling(period);

			AssignmentRepository.Find(new[] { agent }, period, scenario).Count.Should().Be.EqualTo(7);
		}

		public FullSchedulingTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}