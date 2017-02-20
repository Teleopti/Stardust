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
	public class WebOptimizationStardustHandlerTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		private PlanningPeriod planningPeriod;
		public WebOptimizationStardustHandler Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeAgentGroupStaffLoader AgentGroupStaffLoader;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;

		[Test]
		public void ShouldDoOptimization()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var activity = ActivityRepository.Has("_");
			SkillRepository.Has("skill", activity);
			ScenarioRepository.Has("some name");

			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(new JobResult(JobCategory.WebOptimization, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31), PersonFactory.CreatePerson(), DateTime.UtcNow));
			PlanningPeriodRepository.Add(planningPeriod);

			var reqEvent = new WebOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value
			};
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebOptimization).Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldSaveExceptionToJobResult()
		{
			planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			planningPeriod.JobResults.Add(new JobResult(JobCategory.WebOptimization, new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31),
				PersonFactory.CreatePerson(), DateTime.UtcNow));
			PlanningPeriodRepository.Add(planningPeriod);
			var reqEvent = new WebOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriod.Id.Value
			};
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			Target.Handle(reqEvent);

			var jobResultDetail = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebOptimization).Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Null();
		}
	}
}