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
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[DomainTest]
	[ExtendScope(typeof(DayOffOptimizationEventHandler))]
	[ExtendScope(typeof(WebScheduleStardustHandler))]
	[ExtendScope(typeof(SchedulingEventHandler))]
	public class SchedulePlanningPeriodCommandHandlerHeatMapTest : DayOffOptimizationScenario
	{
		public SchedulePlanningPeriodCommandHandler Target;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public FakeJobResultRepository JobResultRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeTenants Tenants;
		public FakeAppDomainPrincipalContext AppDomainPrincipalContext;
		public FakePersonRepository PersonRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		private void setup()
		{
			var teleoptiIdentity = AppDomainPrincipalContext.Current().Identity as TeleoptiIdentity;
			var businessUnit = new Domain.Common.BusinessUnit(teleoptiIdentity.BusinessUnitName);
			businessUnit.SetId(teleoptiIdentity.BusinessUnitId);
			BusinessUnitRepository.Has(businessUnit);
			Tenants.Has(teleoptiIdentity.DataSource.DataSourceName);
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
		}

		[Test]
		public void ShouldCalculateStaffingCorrectly()
		{
			setup();

			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var filterContract = new Contract("_")
			{
				PositiveDayOffTolerance = 0,
				NegativeDayOffTolerance = 0
			};
			var dayOff = new DayOffTemplate(new Description("_"));
			DayOffTemplateRepository.Has(dayOff);
			PersonRepository.Has(filterContract, new ContractScheduleWorkingMondayToFriday(), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, schedulePeriod, ruleSet, skill);

			var planningGroup = new PlanningGroup().AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, SchedulePeriodType.Week, planningGroup);
			PlanningGroupRepository.Has(planningGroup);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				1,
				5,
				5,
				25,
				25));

			Target.Execute(planningPeriod.Id.Value);

			var lastJobResult = JobResultRepository.LoadAll().First();
			var result = JsonConvert.DeserializeObject<FullSchedulingResultModel>(lastJobResult.Details.Last().Message);
			var skillResult = result.SkillResultList.ToList();
			skillResult.Count.Should().Be.EqualTo(1);
			skillResult.First().SkillName.Should().Be.EqualTo(skill.Name);
			var dayCount = skillResult.First().SkillDetails.ToList();
			dayCount[1].RelativeDifference.Should().Be.EqualTo(-1);
			dayCount[2].RelativeDifference.Should().Be.EqualTo(-1);
			dayCount[5].RelativeDifference.Should().Be.EqualTo(-0.96);
			dayCount[6].RelativeDifference.Should().Be.EqualTo(-0.96);
		}

		public SchedulePlanningPeriodCommandHandlerHeatMapTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}