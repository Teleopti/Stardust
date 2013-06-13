﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class PersonRequestRefresherTest
	{
		[Test]
		public void ShouldRefreshOneWhenDuplicateUpdates()
		{
			var updater = MockRepository.GenerateMock<IUpdatePersonRequestsFromMessages>();
			var target = new PersonRequestRefresher(updater);
			var requestId = Guid.NewGuid();
			var request1 = new EventMessage {DomainObjectId = requestId};
			var request2 = new EventMessage {DomainObjectId = requestId};
			var someOtherMessage = new EventMessage {DomainObjectId = Guid.NewGuid()};

			var allMessages = new List<IEventMessage> {request1, request2, someOtherMessage};

			target.Refresh(allMessages,
			               new List<IEventMessage> {request1, request2});

			allMessages.Should().Have.SameValuesAs(someOtherMessage);

			updater.AssertWasCalled(x => x.UpdatePersonRequest(request1));
			updater.AssertWasNotCalled(x => x.UpdatePersonRequest(request2));
			updater.AssertWasNotCalled(x => x.UpdatePersonRequest(someOtherMessage));
		}
	}
}