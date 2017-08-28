using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	[DomainTest]
	public class WebDayoffOptimizationStardustHandlerTest
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

			FakeSchedulingSourceScope.UsedToBe().Should().Be.EqualTo(ScheduleSource.WebScheduling);
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

			Assert.Throws<InvalidOperationException>(() => Target.Handle(reqEvent));

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Empty();

			FakeSchedulingSourceScope.UsedToBe().Should().Be.EqualTo(ScheduleSource.WebScheduling);
		}
	}
}