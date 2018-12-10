using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
	public class DenyRequestCommandHandlerTest : IIsolateSystem
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IPersonRequestCheckAuthorization Authorization;
		public ICurrentScenario CurrentScenario;
		public INow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new ThisIsNow(new DateTime(2017, 1, 1))).For<INow>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario { DefaultScenario = true })).For<IScenarioRepository>();
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

			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization, CurrentScenario);
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

			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization, CurrentScenario);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
			messagePropertyChanged.Should().Be(true);
			personRequest.DenyReason.Should().Be("RequestDenyReasonSupervisor");
			command.IsReplySuccess.Should().Be(true);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldReturnErrorWhenDenyADeniedRequest()
		{
			var personRequest = createPersonRequest();
			personRequest.Deny(string.Empty, Authorization);

			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = "RequestDenyReasonSupervisor"
			};
			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization, CurrentScenario);
			commandHandler.Handle(command);

			personRequest.IsDenied.Should().Be(true);
			command.ErrorMessages.Count.Should().Be(1);
			command.ErrorMessages[0].Should().Be("A request that is Denied cannot be Denied.");
		}

		[Test]
		[SetCulture("en-US")]
		public void CanDenyAWaitlistedRequest()
		{
			var absence = new Absence();
			var personRequest = createPersonRequest();
			personRequest.Person.WorkflowControlSet
				= WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			personRequest.Request = new AbsenceRequest(absence, new DateTimePeriod(2017, 8, 30, 8, 2017, 8, 30, 9));
			personRequest.Deny(string.Empty, Authorization, personRequestDenyOption: PersonRequestDenyOption.AutoDeny);

			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = "RequestDenyReasonSupervisor",
				IsManualDeny = true
			};
			var commandHandler = new DenyRequestCommandHandler(PersonRequestRepository, Authorization, CurrentScenario);
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
