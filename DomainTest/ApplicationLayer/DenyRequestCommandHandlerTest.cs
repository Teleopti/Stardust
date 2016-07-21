﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
	public class DenyRequestCommandHandlerTest
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IPersonRequestCheckAuthorization Authorization;

		[Test]
		public void ShouldDenyPersonRequest()
		{
			var personRequest = createPersonRequest();
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization);
			commandHandler.Handle(command);
			personRequest.IsDenied.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Be(null);
			personRequest.DenyReason.Should().Be("RequestDenyReasonSupervisor");
		}

		[Test]
		public void ShouldUpdateMessageWhenThereIsAReplyMessage()
		{
			var personRequest = createPersonRequest();
			var messagePropertyChanged = false;

			personRequest.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName.Equals("Message", StringComparison.OrdinalIgnoreCase))
				{
					messagePropertyChanged = true;
				}
			};

			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				ReplyMessage = "test"
			};

			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
			messagePropertyChanged.Should().Be(true);
			personRequest.DenyReason.Should().Be("RequestDenyReasonSupervisor");
		}

		private IPersonRequest createPersonRequest()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRequestFactory = new PersonRequestFactory() {Person = person};
			var personRequest = personRequestFactory.CreatePersonRequest(person);
			personRequest.SetId(Guid.NewGuid());
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}
	}
}
