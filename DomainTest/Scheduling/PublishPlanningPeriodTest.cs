using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class PublishPlanningPeriodTest
	{
		[Test]
		public void ShouldPublishPlanningPeriodForGivenPerson()
		{
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2001, 1, 1));
			var planningPeriod =
				new PlanningPeriod(new PlanningPeriodSuggestions(new ThisIsNow(new DateTime(2010, 1, 1)),
					new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			planningPeriod.Publish(person);

			person.WorkflowControlSet.SchedulePublishedToDate.GetValueOrDefault().Should().Be.EqualTo(planningPeriod.Range.EndDate.Date);
			planningPeriod.State.Should().Be.EqualTo(PlanningPeriodState.Published);
		}

		[Test]
		public void ShouldNotPublishPlanningPeriodForGivenPeopleWithoutWorkflowControlSet()
		{
			var person = PersonFactory.CreatePersonWithId();
			var planningPeriod =
				new PlanningPeriod(new PlanningPeriodSuggestions(new ThisIsNow(new DateTime(2010, 1, 1)),
					new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			planningPeriod.Publish(person);

			person.WorkflowControlSet.Should().Be.Null();
		}

		[Test]
		public void ShouldPublishPlanningPeriodForGivenPeople()
		{
			var person1 = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2001, 1, 1));
			var person2 = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2002, 1, 1));
			var planningPeriod =
				new PlanningPeriod(new PlanningPeriodSuggestions(new ThisIsNow(new DateTime(2010, 1, 1)),
					new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			planningPeriod.Publish(person1,person2);

			person1.WorkflowControlSet.SchedulePublishedToDate.GetValueOrDefault().Should().Be.EqualTo(planningPeriod.Range.EndDate.Date);
			person2.WorkflowControlSet.SchedulePublishedToDate.GetValueOrDefault().Should().Be.EqualTo(planningPeriod.Range.EndDate.Date);
		}
	}
}
