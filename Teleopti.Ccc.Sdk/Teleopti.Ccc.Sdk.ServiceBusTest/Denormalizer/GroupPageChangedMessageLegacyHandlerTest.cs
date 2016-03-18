using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class GroupPageChangedMessageLegacyHandlerTest
	{
		[Test]
		public void ShouldHandleLegacyMessage()
		{
			var publisher = MockRepository.GenerateMock<IEventPublisher>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWorkFactory.Stub(x => x.Current())
				.Return(unitOfWorkFactory);

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork())
				.Return(unitOfWork);


			var handler = new PublishGroupPageCollectionChangedFromLegacyGroupPageChangedMessage(publisher, currentUnitOfWorkFactory);
			var legacyMessage = new GroupPageChangedMessage();

			handler.Consume(legacyMessage);

			publisher.AssertWasCalled(x => x.Publish(null),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<IEvent[]>(t => t.Length == 1 && t[0] is GroupPageCollectionChangedEvent))));

			currentUnitOfWorkFactory.AssertWasCalled(x => x.Current());
			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork());
			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}
	}
}