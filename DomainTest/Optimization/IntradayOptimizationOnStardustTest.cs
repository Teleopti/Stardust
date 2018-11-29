using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class IntradayOptimizationOnStardustTest
	{
		public FakeEventPublisher EventPublisher;
		public IntradayOptimizationOnStardust Target;
		public FakeJobResultRepository JobResultRepository;
		public ILoggedOnUser LoggedOnUser;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		
		[Test]
		public void ShouldPublishEventWothPlanningPeriodId()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, SchedulePeriodType.Week, 1);
			
			Target.Execute(planningPeriod.Id.Value);

			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>().Single().PlanningPeriodId
				.Should().Be.EqualTo(planningPeriod.Id.Value);
		}

		[Test]
		public void ShouldIncludeCreatedJobResult()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, SchedulePeriodType.Week, 1);
			
			Target.Execute(planningPeriod.Id.Value);

			var createdJobResult = JobResultRepository.LoadAll().Single();
			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>().Single().JobResultId
				.Should().Be.EqualTo(createdJobResult.Id.Value);
		}

		[Test]
		public void ShouldSetLoggedOnUserOnJobResult()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, SchedulePeriodType.Week, 1);
			var loggedOnPerson = LoggedOnUser.CurrentUser();
			
			Target.Execute(planningPeriod.Id.Value);
			
			var createdJobResult = JobResultRepository.LoadAll().Single();
			loggedOnPerson.Should().Not.Be.Null();
			createdJobResult.Owner.Should().Be.EqualTo(loggedOnPerson);
		}

		[Test]
		public void ShouldSetJobResultPeriodBasedOnPlanningPeriod()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, SchedulePeriodType.Week, 1);
			
			Target.Execute(planningPeriod.Id.Value);
			
			JobResultRepository.LoadAll().Single().Period
				.Should().Be.EqualTo(DateOnlyPeriod.CreateWithNumberOfWeeks(DateOnly.Today, 1));
		}

		[Test]
		public void ShouldAddJobResultToPlanningPeriod()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, SchedulePeriodType.Week, 1);
			
			Target.Execute(planningPeriod.Id.Value);

			planningPeriod.JobResults.Count()
				.Should().Be.EqualTo(1);
		}
	}
}