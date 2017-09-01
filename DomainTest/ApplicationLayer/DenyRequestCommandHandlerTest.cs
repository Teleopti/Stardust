using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
	public class DenyRequestCommandHandlerTest : ISetup
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IPersonRequestCheckAuthorization Authorization;
		public INow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 1, 1))).For<INow>();
		}

		[Test]
		public void ShouldDenyPersonRequest()
		{
			var personRequest = createPersonRequest();
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = "RequestDenyReasonSupervisor"
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
				ReplyMessage = "test",
				DenyReason = "RequestDenyReasonSupervisor"
			};

			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
			messagePropertyChanged.Should().Be(true);
			personRequest.DenyReason.Should().Be("RequestDenyReasonSupervisor");
			command.IsReplySuccess.Should().Be(true);
		}

		[Test]
		[Culture("en-US")]
		public void ShouldReturnErrorWhenDenyADeniedRequest()
		{
			var personRequest = createPersonRequest();
			personRequest.Deny(string.Empty, Authorization);

			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = "RequestDenyReasonSupervisor"
			};
			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			command.ErrorMessages.Count.Should().Be(1);
			command.ErrorMessages[0].Should().Be("A request that is Denied cannot be Denied.");
		}

		[Test]
		[Culture("en-US")]
		public void CanDenyAWaitlistedRequest()
		{
			var absence = new Absence();
			var personRequest = createPersonRequest();
			personRequest.Person.WorkflowControlSet
				= WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			personRequest.Request = new AbsenceRequest(absence, new Interfaces.Domain.DateTimePeriod(2017, 8, 30, 8, 2017, 8, 30, 9));
			personRequest.Deny(string.Empty, Authorization, personRequestDenyOption: PersonRequestDenyOption.AutoDeny);

			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = "RequestDenyReasonSupervisor",
				IsManualDeny = true
			};
			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			personRequest.IsWaitlisted.Should().Be(false);
			command.ErrorMessages.Count.Should().Be(0);
		}

		private IPersonRequest createPersonRequest()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personRequestFactory = new PersonRequestFactory() { Person = person };
			var personRequest = personRequestFactory.CreatePersonRequest(person);
			personRequest.SetId(Guid.NewGuid());
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}
	}
}
