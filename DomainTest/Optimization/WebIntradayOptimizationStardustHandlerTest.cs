using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874)]
	public class WebIntradayOptimizationStardustHandlerTest : ISetup
	{
		public WebIntradayOptimizationStardustHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakeDataSourceForTenant DataSourceForTenant;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeScenarioRepository ScenarioRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			system.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
		}

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
		public void ShouldAddJobResultDetail()
		{
			setDataSource();
			setCurrentPrincipal();
			ScenarioRepository.Has("some name");

			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			var jobResultId = Guid.NewGuid();
			var startDate = new DateOnly(2017, 3, 1);
			var endDate = new DateOnly(2017, 3, 7);
			JobResultRepository.Add(new JobResult(JobCategory.WebIntradayOptimiztion, new DateOnlyPeriod(startDate, endDate), PersonFactory.CreatePerson("name1"), DateTime.Now).WithId(jobResultId));
			Target.Handle(new OptimizationWasOrdered
			{
				JobResultId = jobResultId,
				TotalEvents = 3,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				LogOnDatasource = "Teleopti WFM",
				StartDate = startDate,
				EndDate = endDate
			});

			var jobResult = JobResultRepository.Get(jobResultId);
			jobResult.Details.Count().Should().Be.EqualTo(1);
			jobResult.FinishedOk.Should().Be.False();
		}


		[Test]
		public void ShouldUpdateFinishOkJobResultWhenAllDone()
		{
			setDataSource();
			setCurrentPrincipal();
			ScenarioRepository.Has("some name");

			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			var jobResultId = Guid.NewGuid();
			var startDate = new DateOnly(2017, 3, 1);
			var endDate = new DateOnly(2017, 3, 7);
			JobResultRepository.Add(new JobResult(JobCategory.WebIntradayOptimiztion, new DateOnlyPeriod(startDate, endDate), PersonFactory.CreatePerson("name1"), DateTime.Now).WithId(jobResultId));
			Target.Handle(new OptimizationWasOrdered
			{
				JobResultId = jobResultId,
				TotalEvents = 1,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				LogOnDatasource = "Teleopti WFM",
				StartDate = startDate,
				EndDate = endDate
			});

			var jobResult = JobResultRepository.Get(jobResultId);
			jobResult.Details.Count().Should().Be.EqualTo(1);
			jobResult.FinishedOk.Should().Be.True();
		}
	}
}