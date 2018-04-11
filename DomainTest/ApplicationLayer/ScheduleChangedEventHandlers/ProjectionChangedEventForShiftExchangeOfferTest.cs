using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon.Toggle;
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
		
		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldPublishProjectionChangedEventForShiftExchangeOffer(bool defaultScenario)
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithScenario(scenario, defaultScenario);
			
			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-30 08:00".Utc(),
				EndDateTime = "2016-06-30 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			return Publisher.PublishedEvents.OfType<ProjectionChangedEventForShiftExchangeOffer>().Any();
		}

		[Test]
		public void ShouldSetProperties()
		{			
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithScenario(scenario, true);
			var @event = new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-30 08:00".Utc(),
				EndDateTime = "2016-06-30 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			};
			
			Target.Handle(@event);

			var result = Publisher.PublishedEvents.OfType<ProjectionChangedEventForShiftExchangeOffer>().Single();

			result.PersonId.Should().Be.EqualTo(person);
			result.Days.Last().Checksum.Should().Not.Be.EqualTo(0);
			result.Days.Last().Date.Should().Be.EqualTo("2016-06-30".Utc());
			
		}
	}
}