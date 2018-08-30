using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldHandleAgentsWithDifferentSameStartTime()
		{
			var date = new DateOnly(2017, 9, 25);
			var activity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(0, 0, 16, 0, 60), new TimePeriodWithSegment(8, 0, 24, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)), TimeSpan.FromMinutes(15)));
			SkillDayRepository.Has(
				skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(1), 1, new Tuple<TimePeriod, double>(new TimePeriod(1, 2), 100)),
				skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(2), 1, new Tuple<TimePeriod, double>(new TimePeriod(22, 23), 100)),
				skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(3), 1, new Tuple<TimePeriod, double>(new TimePeriod(1, 2), 100)),
				skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(4), 1, new Tuple<TimePeriod, double>(new TimePeriod(22, 23), 100)),
				skill.CreateSkillDayWithDemandOnInterval(scenario, date.AddDays(5), 1, new Tuple<TimePeriod, double>(new TimePeriod(1, 2), 100))
				);
			var agentWithSameStarttime = PersonRepository.Has(ruleSet, skill);
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithSameStarttime, scenario, date.AddDays(0)).WithDayOff());
			Enumerable.Range(1, 5).ForEach(x =>
			{
				PersonAssignmentRepository.Has(agentWithSameStarttime, scenario, activity, shiftCategory, date.AddDays(x), new TimePeriod(8, 17));
			});
			PersonAssignmentRepository.Has(new PersonAssignment(agentWithSameStarttime, scenario, date.AddDays(6)).WithDayOff());
			var planningGroup = PlanningGroupRepository.Has();
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, planningGroup);
			var planningGroupSettings = PlanningGroupSettings.CreateDefault(planningGroup);
			planningGroupSettings.AddFilter(new TeamFilter(agentWithSameStarttime.MyTeam(date)));
			planningGroupSettings.BlockFinderType = BlockFinderType.BetweenDayOff;
			planningGroupSettings.BlockSameStartTime = true;
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			
			Target.Execute(planningPeriod.Id.Value);
			
			PersonAssignmentRepository.LoadAll().Where(x => x.Person.Equals(agentWithSameStarttime) && x.DayOff()==null)
				.Select(x => x.Period.StartDateTime.TimeOfDay).Distinct().Count()
				.Should().Be.EqualTo(1);
		}
	}
}