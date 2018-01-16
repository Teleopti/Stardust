using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
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
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[TestFixture]
	[DomainTest]
	public class SchedulePlanningPeriodCommandHandlerTest
	{
		public SchedulePlanningPeriodCommandHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeEventPublisher EventPublisher;
		[Test]
		public void ShouldPublishWebScheduleStardustEvent()
		{
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			PlanningPeriodRepository.Add(planningPeriod);
			planningPeriod.JobResults.Count.Should().Be.EqualTo(0);
			Target.Execute(planningPeriod.Id.GetValueOrDefault(), true);

			JobResultRepository.LoadAll()
				.Single(x => x.JobCategory == JobCategory.WebSchedule)
				.Period.Should()
				.Be.EqualTo(planningPeriod.Range);
			planningPeriod.JobResults.Count.Should().Be.EqualTo(1);
			var webScheduleStardustEvent = (EventPublisher.PublishedEvents.Single() as WebScheduleStardustEvent);
			webScheduleStardustEvent.PlanningPeriodId.Should().Be.EqualTo(planningPeriod.Id.GetValueOrDefault());
			webScheduleStardustEvent.Policy.Should().Be.EqualTo(WebScheduleStardustBaseEvent.HalfNodesAffinity);
		}
	}


	public class ResourcePlannerAttribute : DomainTestAttribute
	{
		public FakeEventPublisher Publisher;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			Publisher.AddHandler<WebDayoffOptimizationStardustHandler>();
			Publisher.AddHandler<DayOffOptimizationEventHandler>();
			Publisher.AddHandler<WebScheduleStardustHandler>();
			Publisher.AddHandler<SchedulingEventHandler>();
		}
	}


	[ResourcePlanner]
	public class SchedulePlanningPeriodCommandHandlerHeatMapTest : DayOffOptimizationScenario
	{

		public SchedulePlanningPeriodCommandHandler Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
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


		[Test, Ignore("failed for now")]
		public void ShouldCalculateStaffingCorrectly()
		{
			setup();

			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var irrelevantActivity = ActivityRepository.Has("irrelevant");
			var skill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));
			var irrelevantSkill = SkillRepository.Has("irrelevant skill", irrelevantActivity); // To see we don't include this in the result

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Day, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var filterContract = new ContractWithMaximumTolerance().WithId();
			var agent = PersonRepository.Has(filterContract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var agentOutSideGroup = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill, irrelevantSkill);

			var planningGroup = new PlanningGroup("_").AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, SchedulePeriodType.Day, planningGroup);
			PlanningGroupRepository.Has(planningGroup);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentOutSideGroup, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));

			Target.Execute(planningPeriod.Id.Value, true);

			var lastJobResult = JobResultRepository.LoadAll().First();
			var result = JsonConvert.DeserializeObject<OptimizationResultModel>(lastJobResult.Details.Last().Message);
			var skillResult = result.SkillResultList.ToList();
			skillResult.Count.Should().Be.EqualTo(1);
			skillResult.First().SkillName.Should().Be.EqualTo(skill.Name);
			var dayCount = skillResult.First().SkillDetails.ToList();
			dayCount.Count.Should().Be.EqualTo(1);
			dayCount.First().RelativeDifference.Should().Be.EqualTo(0);
		}

		private void setup()
		{
			var teleoptiIdentity = AppDomainPrincipalContext.Current().Identity as TeleoptiIdentity;
			BusinessUnitRepository.Has(teleoptiIdentity.BusinessUnit);
			Tenants.Has(teleoptiIdentity.DataSource.DataSourceName);
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
		}

		public SchedulePlanningPeriodCommandHandlerHeatMapTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerDayOffOptimizationIslands47208) : base(seperateWebRequest, resourcePlannerDayOffOptimizationIslands47208)
		{
		}
	}

}