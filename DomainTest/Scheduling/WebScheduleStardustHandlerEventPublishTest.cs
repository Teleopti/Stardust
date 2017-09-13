using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class WebScheduleStardustHandlerEventPublishTest : SchedulingScenario, ISetup
	{
		public WebScheduleStardustHandler Target;
		public IFullScheduling FullScheduling;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeEventPublisher EventPublisher;
		public FakeDataSourceForTenant DataSourceForTenant;
		public FakePersonRepository PersonRepository;
		public FakeJobResultRepository JobResultRepository;
		public FakeTenants Tenants;
		public FakeSchedulingSourceScope FakeSchedulingSourceScope;

		private IBusinessUnit businessUnit;

		public WebScheduleStardustHandlerEventPublishTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}

		public void SetUp()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
		}

		[Test]
		public void ShouldPublishOptimizationEvent()
		{
			SetUp();
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var activity = ActivityRepository.Has("_");
			SkillRepository.Has("skill", activity);
			ScenarioRepository.Has("some name");

			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow).WithId(jobResultId);
			planningPeriod.JobResults.Add(jobResult);
			JobResultRepository.Add(jobResult);
			PlanningPeriodRepository.Add(planningPeriod);

			var reqEvent = new WebScheduleStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				JobResultId = jobResultId
			};

			Target.Handle(reqEvent);

			var webOptimizationStardustEvent = EventPublisher.PublishedEvents.Single() as WebDayoffOptimizationStardustEvent;
			webOptimizationStardustEvent.PlanningPeriodId.Should().Be.EqualTo(planningPeriod.Id.Value);
			webOptimizationStardustEvent.JobResultId.Should().Be.EqualTo(jobResult.Id.Value);
			webOptimizationStardustEvent.Policy.Should().Be.EqualTo(WebScheduleStardustBaseEvent.HalfNodesAffinity);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}
	}
}