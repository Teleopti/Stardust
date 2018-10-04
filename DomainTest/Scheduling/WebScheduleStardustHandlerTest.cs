using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class WebScheduleStardustHandlerTest : SchedulingScenario
	{
		public WebScheduleStardustHandler Target;
		public FullScheduling FullScheduling;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeDataSourceForTenant DataSourceForTenant;
		public FakePersonRepository PersonRepository;
		public FakeJobResultRepository JobResultRepository;
		public FakeTenants Tenants;
		public ICurrentSchedulingSource CurrentSchedulingSource;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;

		private IScenario scenario;
		private Person agent;
		private DateOnlyPeriod period;
		private DateOnly firstDay;

		private IBusinessUnit businessUnit;


		public void SetUp()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
			PersonAssignmentRepository.CurrentSchedulingSource = CurrentSchedulingSource;
		}

		private void prepareSchedule()
		{
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			firstDay = new DateOnly(2015, 10, 12);
			period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"),
				new Team {Site = new Site("site")}, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1,
				1,
				1,
				1,
				1,
				1,
				1)
			);
		}

		[Test]
		public void ShouldDoScheduling()
		{
			SetUp();
			prepareSchedule();

			var planningPeriod = new PlanningPeriod(period,SchedulePeriodType.Day, 8);
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, period, agent, DateTime.UtcNow).WithId(jobResultId);
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

			var jobResultFromDb = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule);
			jobResultFromDb.FinishedOk.Should().Be.True();
			var jobResultDetail = jobResultFromDb.Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Info);
			jobResultDetail.ExceptionMessage.Should().Be.Null();
			jobResultDetail.Message.Should().Not.Be.Null();

			var personAssignments = PersonAssignmentRepository.Find(new[] { agent }, period, scenario);
			personAssignments.All(x => x.Source == ScheduleSource.WebScheduling).Should().Be.True();
		}

		[Test]
		public void ShouldSaveExceptionToJobResult()
		{
			SetUp();
			var planningPeriod = new PlanningPeriod(period,SchedulePeriodType.Day, 8);
			var jobResultId = Guid.NewGuid();
			var jobResult = new JobResult(JobCategory.WebSchedule, period, agent, DateTime.UtcNow).WithId(jobResultId);
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

			Assert.Throws<NoDefaultScenarioException>(() => Target.Handle(reqEvent));

			var jobResultFromDb = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule);
			jobResultFromDb.FinishedOk.Should().Be.False();
			var jobResultDetail = jobResultFromDb.Details.Single();
			jobResultDetail.DetailLevel.Should().Be.EqualTo(DetailLevel.Error);
			jobResultDetail.ExceptionMessage.Should().Not.Be.Null();
			jobResultDetail.Message.Should().Be.Empty();

			var personAssignments = PersonAssignmentRepository.Find(new[] { agent }, period, scenario);
			personAssignments.All(x => x.Source == ScheduleSource.WebScheduling).Should().Be.True();
		}

		public WebScheduleStardustHandlerTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}