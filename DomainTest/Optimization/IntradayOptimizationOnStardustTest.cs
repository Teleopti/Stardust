using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
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
		
		[Test]
		public void ShouldPublishEvent()
		{
			var planningPeriodId = Guid.NewGuid();

			Target.Execute(planningPeriodId);

			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>().Single().PlanningPeriodId
				.Should().Be.EqualTo(planningPeriodId);
		}

		[Test]
		public void ShouldIncludeCreatedJobResult()
		{
			Target.Execute(Guid.NewGuid());

			var createdJobResult = JobResultRepository.LoadAll().Single();
			
			EventPublisher.PublishedEvents.OfType<IntradayOptimizationOnStardustWasOrdered>().Single().JobResultId
				.Should().Be.EqualTo(createdJobResult.Id.Value);
		}

		[Test]
		public void ShouldSetLoggedOnUserOnJobResult()
		{
			var loggedOnPerson = LoggedOnUser.CurrentUser();
			
			Target.Execute(Guid.NewGuid());
			
			var createdJobResult = JobResultRepository.LoadAll().Single();
			loggedOnPerson.Should().Not.Be.Null();
			createdJobResult.Owner.Should().Be.EqualTo(loggedOnPerson);
		}
	}
}