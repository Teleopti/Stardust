using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class ReplyRequestCommandHandlerTest
	{
		const string replyMessage = "New comment\r\nLine 1\r\nLine2";

		private MockRepository _mock;
		private IPersonRequestRepository _personRequestRepository;
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
				Message = replyMessage
			};
		}

		[Test]
		public void ShouldReplyRequestSuccessfully()
		{
			var request = _mock.StrictMock<IPersonRequest>();

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
	}
}