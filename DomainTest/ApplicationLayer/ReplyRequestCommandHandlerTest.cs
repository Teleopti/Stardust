using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class ReplyRequestCommandHandlerTest
	{
		const string replyMessage = "New comment\r\nLine 1\r\nLine2";

		private MockRepository _mock;
		private IPersonRequestRepository _personRequestRepository;
		private IPersonRequestRepository _personRequestRepositoryNoMock;
		private ReplyRequestCommandHandler _target;
		private ReplyRequestCommand _replyRequestCommand;
		private Guid requestId;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
			_target = new ReplyRequestCommandHandler(_personRequestRepository);

			requestId = Guid.NewGuid();
			_replyRequestCommand = new ReplyRequestCommand
			{
				PersonRequestId = requestId,
				ReplyMessage = replyMessage
			};
			
			_personRequestRepositoryNoMock = new FakePersonRequestRepository();
		}
		
		[Test]
		public void ShouldReplyRequestSuccessfully()
		{
			var request = _mock.StrictMock<IPersonRequest>();
			request.Stub(r => r.Id).Return(new Guid());
			request.Stub(r => r.GetMessage(new NoFormatting())).IgnoreArguments().Return(replyMessage);
			request.Stub(r => r.CheckReplyTextLength("")).IgnoreArguments().Return(true);
			using (_mock.Record())
			{
				Expect.Call(_personRequestRepository.Get(requestId)).Return(request);
				Expect.Call(request.Reply(replyMessage)).Return(true);
			}
			using (_mock.Playback())
			{
				_target.Handle(_replyRequestCommand);
			}
		}
		[Test]
		public void ShouldAddToErrorMessageAfterMessageLengthExceed()
		{
			var personRequest = new PersonRequestFactory().CreatePersonRequest().WithId();
			_personRequestRepositoryNoMock.Add(personRequest);
			var veryLong = new string('w', 2001);
			var command = new ReplyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				ReplyMessage = veryLong
			};
			var commandHandler = new ReplyRequestCommandHandler(_personRequestRepositoryNoMock);

			commandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains("Cannot save this reply as the complete message exceeds the 2000 character limit"));
		}
		[Test]
		public void ShouldHandleExceptionWhenReplyToAutoDeniedRequest()
		{
			var autoDeniedPersonRequest = new PersonRequestFactory().CreateNewPersonRequest().WithId();
			autoDeniedPersonRequest.Deny(null, new PersonRequestAuthorizationCheckerForTest());
			autoDeniedPersonRequest.Persisted();
			_personRequestRepositoryNoMock.Add(autoDeniedPersonRequest);
			var command = new ReplyRequestCommand
			{
				PersonRequestId = autoDeniedPersonRequest.Id.GetValueOrDefault(),
				ReplyMessage = "wee"
			};
			var commandHandler = new ReplyRequestCommandHandler(_personRequestRepositoryNoMock);

			commandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains(string.Format("Cannot save your reply to this request as it has been {0}", autoDeniedPersonRequest.StatusText)));
		}
		[Test]
		public void ShouldHandleExceptionWhenReplyToDeniedRequest()
		{
			var deniedPersonRequest = new PersonRequestFactory().CreatePersonRequest().WithId();
			deniedPersonRequest.Deny(null, new PersonRequestAuthorizationCheckerForTest());
			deniedPersonRequest.Persisted();
			_personRequestRepositoryNoMock.Add(deniedPersonRequest);
			var command = new ReplyRequestCommand
			{
				PersonRequestId = deniedPersonRequest.Id.GetValueOrDefault(),
				ReplyMessage = "wee"
			};
			var commandHandler = new ReplyRequestCommandHandler(_personRequestRepositoryNoMock);

			commandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains(string.Format("Cannot save your reply to this request as it has been {0}", deniedPersonRequest.StatusText)));
		}
		[Test]
		public void ShouldHandleExceptionWhenReplyToCancelledRequest()
		{
			var absence = new Absence();
			var absenceRequest = new PersonRequestFactory().CreateAbsenceRequest(absence, new DateTimePeriod());
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId(new Guid());
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			personRequest.Cancel(new PersonRequestAuthorizationCheckerForTest());
			personRequest.Persisted();
			_personRequestRepositoryNoMock.Add(personRequest);
			var command = new ReplyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				ReplyMessage = "wee"
			};
			var commandHandler = new ReplyRequestCommandHandler(_personRequestRepositoryNoMock);

			commandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains(string.Format("Cannot save your reply to this request as it has been {0}", personRequest.StatusText)));
		}
		[Test]
		public void ShouldSetReplyMessageToRequest()
		{
			var personRequest = new PersonRequestFactory().CreatePersonRequest().WithId();
			personRequest.Pending();
			personRequest.Persisted();
			_personRequestRepositoryNoMock.Add(personRequest);
			var command = new ReplyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				ReplyMessage = "wee"
			};
			var commandHandler = new ReplyRequestCommandHandler(_personRequestRepositoryNoMock);

			commandHandler.Handle(command);

			Assert.IsTrue(personRequest.GetMessage(new NoFormatting())== "\r\nwee");
			Assert.IsTrue(command.IsReplySuccess);
		}
	}
}