using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	public class ProjectionChangedEventLightPublisherTest
	{
		public ProjectionChangedEventLightPublisher Target;
		public FakeEventPublisher Publisher;
		
		[Test]
		public void ShouldBePublished()
		{
			Target.Handle(new ScheduleChangedEvent());

			Publisher.PublishedEvents.OfType<ProjectionChangedEventLight>().Count()
				.Should().Be.EqualTo(1);
		}
	}
}