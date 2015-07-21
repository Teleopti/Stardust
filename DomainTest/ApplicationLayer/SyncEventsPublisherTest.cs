using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class SyncEventsPublisherTest
	{
		[Test]
		public void ShouldPublishEachEvent()
		{
			var eventPublisher = MockRepository.GenerateMock<IEventPopulatingPublisher>();
			var target = new SyncEventsPublisher(eventPublisher);
			var events = new[] {new Event(), new Event()};

			target.Publish(events);

			eventPublisher.AssertWasCalled(x => x.Publish(events.ToArray()));
		}
	}
}