using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
{
	[TestFixture]
	public class PersonRequestRefresherTest
	{
		[Test]
		public void ShouldRefreshOneWhenDuplicateUpdates()
		{
			var updater = MockRepository.GenerateMock<IUpdatePersonRequestsFromMessages>();
			var remover = MockRepository.GenerateMock<IMessageQueueRemoval>();
			var target = new PersonRequestRefresher(updater, remover);
			var requestId = Guid.NewGuid();
			var request1 = new EventMessage {DomainObjectId = requestId};
			var request2 = new EventMessage {DomainObjectId = requestId};

			var allMessages = new List<IEventMessage> {request1, request2};

			target.Refresh(allMessages);

			updater.AssertWasCalled(x => x.UpdatePersonRequest(request1));
			updater.AssertWasNotCalled(x => x.UpdatePersonRequest(request2));
			remover.AssertWasCalled(x => x.Remove(request1));
		}
	}
}