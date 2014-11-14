using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class SyncEventsPublisherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldPublishEachEvent()
		{
			var eventPublisher = MockRepository.GenerateMock<IEventPopulatingPublisher>();
			var target = new SyncEventsPublisher(eventPublisher);
			var events = new[] {new Event(), new Event()};

			target.Publish(events);

			eventPublisher.AssertWasCalled(x => x.Publish(events[0]));
			eventPublisher.AssertWasCalled(x => x.Publish(events[1]));
		}
	}
}