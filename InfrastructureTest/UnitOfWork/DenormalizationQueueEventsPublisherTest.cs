using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class DenormalizationQueueEventsPublisherTest
	{
		[Test]
		public void ShouldSaveToDenormalizationQueue()
		{
			var saveToQueue = MockRepository.GenerateMock<ISaveToDenormalizationQueue>();
			var target = new DenormalizationQueueEventsPublisher(saveToQueue);
			var events = new[] {new Event(), new Event()};

			target.Publish(events);

			saveToQueue.AssertWasCalled(x => x.Execute(Arg<EventsMessage>.Matches(m => MatchMessage(m, events))));
		}

		private bool MatchMessage(EventsMessage eventsMessage, Event[] events)
		{
			eventsMessage.Events.Should().Have.SameValuesAs(events);
			return true;
		}
	}

}