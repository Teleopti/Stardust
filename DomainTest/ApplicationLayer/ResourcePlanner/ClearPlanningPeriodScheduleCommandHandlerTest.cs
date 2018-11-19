using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[DomainTest]
	public class ClearPlanningPeriodScheduleCommandHandlerTest
	{
		public ClearPlanningPeriodScheduleCommandHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeEventPublisher EventPublisher;
		
		[Test]
		public void ShouldPublishWebClearScheduleStardustEvent()
		{
			var planningPeriod = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());
			PlanningPeriodRepository.Add(planningPeriod);
			planningPeriod.JobResults.Count.Should().Be.EqualTo(0);
			Target.Execute(planningPeriod.Id.GetValueOrDefault());

			JobResultRepository.LoadAll()
				.Single(x => x.JobCategory == JobCategory.WebClearSchedule)
				.Period.Should()
				.Be.EqualTo(planningPeriod.Range);
			planningPeriod.JobResults.Count.Should().Be.EqualTo(1);
			var webClearScheduleStardustEvent = EventPublisher.PublishedEvents.OfType<WebClearScheduleStardustEvent>().Single();
			webClearScheduleStardustEvent.PlanningPeriodId.Should().Be.EqualTo(planningPeriod.Id.GetValueOrDefault());
		}
	}
}