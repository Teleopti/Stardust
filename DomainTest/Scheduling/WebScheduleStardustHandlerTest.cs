using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	[DomainTest]
	public class WebScheduleStardustHandlerTest: ISetup
	{
		public WebScheduleStardustHandler Target;
		public FullScheduling FullScheduling;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeAgentGroupStaffLoader AgentGroupStaffLoader;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		private PlanningPeriod planningPeriod;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldDoScheduling()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var activity = ActivityRepository.Has("_");
			SkillRepository.Has("skill", activity);
			ScenarioRepository.Has("some name");

			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow));
			PlanningPeriodRepository.Add(planningPeriod);
			var reqEvent = new WebScheduleStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value
			};
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);
			
			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule).Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSaveExceptionToJobResult()
		{
			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31),
				PersonFactory.CreatePerson(), DateTime.UtcNow));
			PlanningPeriodRepository.Add(planningPeriod);
			var reqEvent = new WebScheduleStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value
			};
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule).Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Null();
		}

		[Test]
		public void ShouldPublishOptimizationEvent()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var activity = ActivityRepository.Has("_");
			SkillRepository.Has("skill", activity);
			ScenarioRepository.Has("some name");

			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31),
				PersonFactory.CreatePerson(), DateTime.UtcNow));
			PlanningPeriodRepository.Add(planningPeriod);
			var reqEvent = new WebScheduleStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value
			};
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Target.Handle(reqEvent);

			var webScheduleJobResult = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule);
			var webOptimizationJobResult = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebOptimization);
			webOptimizationJobResult.Should().Not.Be.Null();
			webOptimizationJobResult.Owner.Should().Be.EqualTo(webScheduleJobResult.Owner);
			webOptimizationJobResult.Period.Should().Be.EqualTo(webScheduleJobResult.Period);
			EventPublisher.PublishedEvents.Single().GetType().Should().Be.EqualTo(typeof(WebOptimizationStardustEvent));
			(EventPublisher.PublishedEvents.Single() as WebOptimizationStardustEvent).PlanningPeriodId.Should()
				.Be.EqualTo(planningPeriod.Id.Value);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}
	}
}