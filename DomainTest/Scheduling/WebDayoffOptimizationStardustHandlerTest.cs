using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
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
	public class WebDayoffOptimizationStardustHandlerTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			system.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
		}

		private PlanningPeriod planningPeriod;
		public WebDayoffOptimizationStardustHandler Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeAgentGroupStaffLoader AgentGroupStaffLoader;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeDataSourceForTenant DataSourceForTenant;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakePersonRepository PersonRepository;
		public FakeJobResultRepository JobResultRepository;

		private void setDataSource()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSource.Stub(d => d.DataSourceName).Return("Teleopti WFM");
			dataSource.Stub(d => d.Application).Return(new FakeUnitOfWorkFactory());
			DataSourceForTenant.Has(dataSource);
		}

		private IPerson createPerson()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.WorkflowControlSet = new WorkflowControlSet();
			PersonRepository.Add(person);
			return person;
		}

		private void setCurrentPrincipal()
		{
			var person = createPerson();
			var identity = new TeleoptiIdentity("testPerson", null, null, null, null);
			CurrentTeleoptiPrincipal.SetCurrentPrincipal(new TeleoptiPrincipal(identity, person));
		}

		[Test]
		public void ShouldDoOptimization()
		{
			setDataSource();
			setCurrentPrincipal();
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var activity = ActivityRepository.Has("_");
			SkillRepository.Has("skill", activity);
			ScenarioRepository.Has("some name");

			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow).WithId(jobResultId);
			planningPeriod.JobResults.Add(jobResult);
			PlanningPeriodRepository.Add(planningPeriod);
			JobResultRepository.Add(jobResult);

			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			var reqEvent = new WebDayoffOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				JobResultId = jobResultId
			};
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldSaveExceptionToJobResult()
		{
			setDataSource();
			setCurrentPrincipal();
			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow).WithId(jobResultId);
			planningPeriod.JobResults.Add(jobResult);
			PlanningPeriodRepository.Add(planningPeriod);
			JobResultRepository.Add(jobResult);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			var reqEvent = new WebDayoffOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value,
				InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
				LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				JobResultId = jobResultId
			};
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Assert.Throws<InvalidOperationException>(() => Target.Handle(reqEvent));

			var jobResultDetail = planningPeriod.JobResults.Single().Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Null();
		}
	}
}