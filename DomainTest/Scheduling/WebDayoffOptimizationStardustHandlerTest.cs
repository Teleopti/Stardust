using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class WebDayOffOptimizationStardustHandlerTest : DayOffOptimizationScenario
	{
		public WebDayoffOptimizationStardustHandler Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeJobResultRepository JobResultRepository;
		public FakeTenants Tenants;
		public FakeSchedulingSourceScope FakeSchedulingSourceScope;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		public ReduceIslandsLimits ReduceIslandsLimits;

		private IBusinessUnit businessUnit;

		public void SetUp()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
		}

		[Test]
		public void ShouldSetDatasourceCorrectlyForMultipleIslands()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 1);

			Tenants.Has("Teleopti WFM");
			var activity = ActivityRepository.Has("_");
			var skill = new Skill("_") { Activity = activity }.WithId(Guid.NewGuid());
			var skill2 = new Skill("_") { Activity = activity }.WithId(Guid.NewGuid());

			SkillRepository.Has(skill);
			ScenarioRepository.Has("some name");
			
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015, 05, 01), new List<ISkill> { skill }).WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015, 05, 01), new List<ISkill> { skill2 }).WithId(Guid.NewGuid());
			PersonRepository.Add(person2);
			
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());

			var planningGroup = new PlanningGroup("test1");
			planningGroup.AddFilter(new SkillFilter(skill));
			planningGroup.AddFilter(new SkillFilter(skill2));
			var planningPeriod =
				new PlanningPeriod(
					new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()),
					planningGroup);
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow).WithId(jobResultId);
			planningPeriod.JobResults.Add(jobResult);
			PlanningPeriodRepository.Add(planningPeriod);
			JobResultRepository.Add(jobResult);

			var reqEvent = new WebDayoffOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				JobResultId = jobResultId
			};

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();

			FakeSchedulingSourceScope.UsedToBe().ForEach(x => x.Should().Be.EqualTo(ScheduleSource.WebScheduling));
		}

		[Test]
		public void ShouldDoOptimization()
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
			PlanningPeriodRepository.Add(planningPeriod);
			JobResultRepository.Add(jobResult);

			var reqEvent = new WebDayoffOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				JobResultId = jobResultId
			};

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();

			FakeSchedulingSourceScope.UsedToBe().ForEach(x => x.Should().Be.EqualTo(ScheduleSource.WebScheduling));
		}


		[Test]
		public void ShouldSaveExceptionToJobResult()
		{
			SetUp();
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow).WithId(jobResultId);
			planningPeriod.JobResults.Add(jobResult);
			PlanningPeriodRepository.Add(planningPeriod);
			JobResultRepository.Add(jobResult);
			var reqEvent = new WebDayoffOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				JobResultId = jobResultId
			};

			Assert.Throws<NoDefaultScenarioException>(() => Target.Handle(reqEvent));

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Empty();

			FakeSchedulingSourceScope.UsedToBe().ForEach(x => x.Should().Be.EqualTo(ScheduleSource.WebScheduling));
		}

		public WebDayOffOptimizationStardustHandlerTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerDayOffOptimizationIslands47208) : base(seperateWebRequest, resourcePlannerDayOffOptimizationIslands47208)
		{
		}
	}
}