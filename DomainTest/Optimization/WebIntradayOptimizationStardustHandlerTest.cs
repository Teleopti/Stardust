using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class WebIntradayOptimizationStardustHandlerTest: IntradayOptimizationScenarioTest
	{
		public WebIntradayOptimizationStardustHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeTenants Tenants;
		public FakeSchedulingSourceScope FakeSchedulingSourceScope;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldUpdateFinishOkJobResultWhenAllDone()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			ScenarioRepository.Has("some name");
			var jobResultId = Guid.NewGuid();
			var startDate = new DateOnly(2017, 3, 1);
			var endDate = new DateOnly(2017, 3, 7);
			JobResultRepository.Add(new JobResult(JobCategory.WebIntradayOptimization, new DateOnlyPeriod(startDate, endDate), PersonFactory.CreatePerson("name1"), DateTime.Now).WithId(jobResultId));
			var planningPeriod = PlanningPeriodRepository.Has(startDate, SchedulePeriodType.Week, 1);
			Target.Handle(new IntradayOptimizationOnStardustWasOrdered
			{
				JobResultId = jobResultId,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				LogOnDatasource = "Teleopti WFM",
				PlanningPeriodId = planningPeriod.Id.Value
			});

			var jobResult = JobResultRepository.Get(jobResultId);
			jobResult.Details.Count().Should().Be.EqualTo(1);
			jobResult.FinishedOk.Should().Be.True();

			FakeSchedulingSourceScope.UsedToBe().ForEach(x => x.Should().Be.EqualTo(ScheduleSource.WebScheduling));
		}
		
		[Test]
		public void ShouldNotSetFinishOkJobResultWhenExceptionIsThrown()
		{
			//AsSystem
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			ScenarioRepository.Has("some name");
			//
			var jobResult = new JobResult(string.Empty, new DateOnlyPeriod(),new Person(), DateTime.UtcNow);
			JobResultRepository.Add(jobResult);
			var nonExistingPlanningPeriodId = Guid.NewGuid();

			
			Assert.That(() =>
			{
				Target.Handle(new IntradayOptimizationOnStardustWasOrdered
				{
					JobResultId = jobResult.Id.Value,
					LogOnBusinessUnitId = businessUnit.Id.Value,
					LogOnDatasource = "Teleopti WFM",
					PlanningPeriodId = nonExistingPlanningPeriodId
				});
				
			}, Throws.Exception);	

			jobResult.Details.Any().Should().Be.True();
			jobResult.FinishedOk.Should().Be.False();
		}
	}
}