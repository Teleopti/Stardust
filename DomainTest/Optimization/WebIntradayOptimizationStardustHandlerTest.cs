using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	[Toggle(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874)]
	public class WebIntradayOptimizationStardustHandlerTest: IntradayOptimizationScenarioTest
	{
		public WebIntradayOptimizationStardustHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeTenants Tenants;
		public FakeSchedulingSourceScope FakeSchedulingSourceScope;

		private IBusinessUnit businessUnit;

		public WebIntradayOptimizationStardustHandlerTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508) : base(resourcePlannerMergeTeamblockClassicIntraday45508)
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
		public void ShouldAddJobResultDetail()
		{
			SetUp();
			ScenarioRepository.Has("some name");
			var jobResultId = Guid.NewGuid();
			var startDate = new DateOnly(2017, 3, 1);
			var endDate = new DateOnly(2017, 3, 7);
			JobResultRepository.Add(new JobResult(JobCategory.WebIntradayOptimiztion, new DateOnlyPeriod(startDate, endDate), PersonFactory.CreatePerson("name1"), DateTime.Now).WithId(jobResultId));
			Target.Handle(new WebIntradayOptimizationStardustEvent
			{
				JobResultId = jobResultId,
				TotalEvents = 3,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				LogOnDatasource = "Teleopti WFM",
				IntradayOptimizationWasOrdered = new IntradayOptimizationWasOrdered
				{
					StartDate = startDate,
					EndDate = endDate
				}
			});

			var jobResult = JobResultRepository.Get(jobResultId);
			jobResult.Details.Count().Should().Be.EqualTo(1);
			jobResult.FinishedOk.Should().Be.False();

			FakeSchedulingSourceScope.UsedToBe().Should().Be.EqualTo(ScheduleSource.WebScheduling);
		}

		[Test]
		public void ShouldUpdateFinishOkJobResultWhenAllDone()
		{
			SetUp();
			ScenarioRepository.Has("some name");
			var jobResultId = Guid.NewGuid();
			var startDate = new DateOnly(2017, 3, 1);
			var endDate = new DateOnly(2017, 3, 7);
			JobResultRepository.Add(new JobResult(JobCategory.WebIntradayOptimiztion, new DateOnlyPeriod(startDate, endDate), PersonFactory.CreatePerson("name1"), DateTime.Now).WithId(jobResultId));
			Target.Handle(new WebIntradayOptimizationStardustEvent
			{
				JobResultId = jobResultId,
				TotalEvents = 1,
				LogOnBusinessUnitId = businessUnit.Id.Value,
				LogOnDatasource = "Teleopti WFM",
				IntradayOptimizationWasOrdered = new IntradayOptimizationWasOrdered
				{
					StartDate = startDate,
					EndDate = endDate
				}
			});

			var jobResult = JobResultRepository.Get(jobResultId);
			jobResult.Details.Count().Should().Be.EqualTo(1);
			jobResult.FinishedOk.Should().Be.True();

			FakeSchedulingSourceScope.UsedToBe().Should().Be.EqualTo(ScheduleSource.WebScheduling);
		}
	}
}