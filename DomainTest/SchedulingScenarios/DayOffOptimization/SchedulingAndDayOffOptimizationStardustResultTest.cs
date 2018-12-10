using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[ExtendScope(typeof(WebScheduleStardustHandler))]
	[Ignore("This should be green when we fix heatmap for real")]
	public class SchedulingAndDayOffOptimizationStardustResultTest : DayOffOptimizationScenario
	{
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public FakeJobResultRepository JobResultRepository;

		public SchedulePlanningPeriodCommandHandler Target;

		[TestCase("Mountain Standard Time", "Mountain Standard Time")]
		[TestCase("Mountain Standard Time", "UTC")]
		public void ShouldHandleTimeZoneIssues(string skillTimeZoneStr, string assignedShiftsTimeZone)
		{
			//TODO: Make attribute for this maybe?
			var systemUser = new Person().WithName(new Name("system","system")).WithId(SystemUser.Id)
				.InTimeZone(TimeZoneInfo.Utc);
			PersonRepository.Has(systemUser);
			BusinessUnitRepository.Has(CurrentBusinessUnit.Current());
			//
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skillTimeZone = TimeZoneInfo.FindSystemTimeZoneById(skillTimeZoneStr);
			var skill = SkillRepository.Has("relevant skill", activity).InTimeZone(skillTimeZone);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Day, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var filterContract = new ContractWithMaximumTolerance().WithId();
			var agent = PersonRepository.Has(filterContract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill)
				.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(assignedShiftsTimeZone));
			var planningGroup = new PlanningGroup().AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, SchedulePeriodType.Day, planningGroup);
			PlanningGroupRepository.Has(planningGroup);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, date.ToDateOnlyPeriod(), new TimePeriod(0, 24));

			Target.Execute(planningPeriod.Id.Value);

			var result = JsonConvert.DeserializeObject<FullSchedulingResultModel>(
					JobResultRepository.LoadAllWithNoLock().Single().Details.Last().Message); 
			var skillResult = result.SkillResultList.ToList();
			var endUserDate = skillResult.First().SkillDetails.First();
			endUserDate.RelativeDifference.Should().Be(0);
		}
		
		public SchedulingAndDayOffOptimizationStardustResultTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}