using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class PersonChangedMessageLegacyHandlerTest
	{
		[Test]
		public void ShouldHandleLegacyMessage()
		{
			var publisher = MockRepository.GenerateMock<IEventPublisher>();
			var handler = new PersonCollectionChangedEventPublisherOfLegacyPersonChangedMessage(publisher);
			var legacyMessage = new PersonChangedMessage();

			handler.Handle(legacyMessage);

			publisher.AssertWasCalled(x => x.Publish(null),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<IEvent[]>(t => t.Length == 1 && t[0] is PersonCollectionChangedEvent))));
		}
	}
}