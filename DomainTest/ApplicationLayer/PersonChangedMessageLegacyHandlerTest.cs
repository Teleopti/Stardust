using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class PersonChangedMessageLegacyHandlerTest
	{
		[Test]
		public void ShouldHandleLegacyMessage()
		{
			var repository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			var handler = new UpdateFindPersonConsumer(repository);
			var legacyMessage = new PersonChangedMessage();

			handler.Handle(legacyMessage);

			handler.AssertWasCalled(x => x.Handle(legacyMessage));
		}
	}
}