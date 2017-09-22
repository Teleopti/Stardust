using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MessageControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnMessagePartialView()
		{
			using (var target = new MessageController(MockRepository.GenerateMock<IMessageViewModelFactory>(), null, null))
			{
				var result = target.Index();

				result.ViewName.Should().Be.EqualTo("MessagePartial");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForMessages()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			using (var target = new MessageController(viewModelFactory, null, null))
			{
				var model = new MessageViewModel[] { };
				var paging = new Paging();

				viewModelFactory.Stub(x => x.CreatePageViewModel(paging)).Return(model);

				var result = target.Messages(paging);

				result.Data.Should().Be.SameInstanceAs(model);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReplyToSimpleMessageWithResultFromPersister()
		{
			var pushMessageDialoguePersister = MockRepository.GenerateMock<IPushMessageDialoguePersister>();
			using (var target = new MessageController(null, pushMessageDialoguePersister, null))
			{
				var messageId = Guid.NewGuid();
				var messageViewModel = new MessageViewModel { MessageId = messageId.ToString() };
				var confirmMessageViewModel = new ConfirmMessageViewModel() { Id = messageId };
				pushMessageDialoguePersister.Stub(x => x.PersistMessage(confirmMessageViewModel)).Return(messageViewModel);

				var result = target.Reply(confirmMessageViewModel);
				result.Data.Should().Be.SameInstanceAs(messageViewModel);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnMessageCount()
		{
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			using (var target = new MessageController(null, null, pushMessageProvider))
			{
				var messageInfo = new MessagesInformationViewModel { UnreadMessagesCount = 1 };
				pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(messageInfo.UnreadMessagesCount);

				var result = target.MessagesCount();

				((MessagesInformationViewModel)result.Data).UnreadMessagesCount.Should().Be.EqualTo(messageInfo.UnreadMessagesCount);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnMessageCountAndMessageAskedFor()
		{
			var messageId = Guid.NewGuid();
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			var messageViewModel = new MessageViewModel { MessageId = messageId.ToString() };
			var messageInfo = new MessagesInformationViewModel
								{
									UnreadMessagesCount = 1,
									MessageItem = messageViewModel
								};

			viewModelFactory.Stub(x => x.CreateMessagesInformationViewModel(new Guid(messageViewModel.MessageId))).Return(messageInfo);

			using (var target = new MessageController(viewModelFactory, null, null))
			{
				var result = target.Message(messageId);
				result.Data.Should().Be.SameInstanceAs(messageInfo);				
			}

		}


		[Test]
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			using (var target = new StubbingControllerBuilder().CreateController<MessageController>(null, null, null))
			{
				const string message = "Test model validation error";
				target.ModelState.AddModelError("Test", message);

				var result = target.Reply(new ConfirmMessageViewModel());
				var data = result.Data as ModelStateResult;

				target.Response.StatusCode.Should().Be(400);
				target.Response.TrySkipIisCustomErrors.Should().Be.True();
				data.Errors.Single().Should().Be(message);
			}
		}

		[Test]
		public void SendNewPushMessageToLoggedOnUser()
		{
			var title = "Title of message...";
			var message = "Body of message...";
			var pushMessageDialoguePersister = MockRepository.GenerateMock<IPushMessageDialoguePersister>();
			using (var target = new MessageController(null, pushMessageDialoguePersister, null))
			{
				pushMessageDialoguePersister.Expect(p => p.SendNewPushMessageToLoggedOnUser(title, message)).Repeat.Once();

				target.Send(title, message);
				pushMessageDialoguePersister.VerifyAllExpectations();
			}
		}
	}
}
