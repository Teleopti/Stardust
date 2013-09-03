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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSaveToDenormalizationQueue()
		{
			var saveToQueue = MockRepository.GenerateMock<ISaveToDenormalizationQueue>();
			var target = new DenormalizationQueueEventsPublisher(saveToQueue, MockRepository.GenerateMock<ISendDenormalizeNotification>());
			var events = new[] {new Event(), new Event()};

			target.Publish(events);

			saveToQueue.AssertWasCalled(x => x.Execute(Arg<EventsPackageMessage>.Matches(m => matchMessage(m, events))));
		}

		private static bool matchMessage(EventsPackageMessage eventsPackageMessage, Event[] events)
		{
			eventsPackageMessage.Events.Should().Have.SameValuesAs(events);
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSendDenormalizationQueueNotification()
		{
			var notifier = MockRepository.GenerateMock<ISendDenormalizeNotification>();
			var target = new DenormalizationQueueEventsPublisher(MockRepository.GenerateMock<ISaveToDenormalizationQueue>(), notifier);
			var events = new[] { new Event() };

			target.Publish(events);

			notifier.AssertWasCalled(x => x.Notify());
		}
	}

}