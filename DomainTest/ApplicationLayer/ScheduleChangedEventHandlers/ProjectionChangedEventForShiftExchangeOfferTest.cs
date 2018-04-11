using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	[DefaultData]
	public class ProjectionChangedEventForShiftExchangeOfferTest
	{
		public ProjectionChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public FakeDatabase Database;
		public MutableNow Now;
		
		[Test]
		public void ShouldPublishProjectionChangedEventForShiftExchangeOffer()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithScenario(scenario);
			
			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-30 08:00".Utc(),
				EndDateTime = "2016-06-30 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			Publisher.PublishedEvents.OfType<ProjectionChangedEventForShiftExchangeOffer>().Count()
				.Should().Be.EqualTo(1);
		}
	}
}