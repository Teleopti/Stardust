using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MessageControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnMessagePartialView()
		{
			var target = new MessageController(MockRepository.GenerateMock<IMessageViewModelFactory>(), null, null);

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("MessagePartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForMessages()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			var target = new MessageController(viewModelFactory, null, null);
			var model = new MessageViewModel[] { };
			var paging = new Paging();

			viewModelFactory.Stub(x => x.CreatePageViewModel(paging)).Return(model);

			var result = target.Messages(paging);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldReplyToSimpleMessage()
		{
			var pushMessageDialoguePersister = MockRepository.GenerateMock<IPushMessageDialoguePersister>();

			var target = new MessageController(null, pushMessageDialoguePersister, null);

			var messageViewModel = new MessageViewModel { MessageId = Guid.NewGuid().ToString() };
			var form = new MessageForm { MessageId = messageViewModel.MessageId };

			pushMessageDialoguePersister.Stub(x => x.Persist(messageViewModel.MessageId)).Return(messageViewModel);

			var result = target.Reply(form);

			result.Data.Should().Be.SameInstanceAs(messageViewModel);
		}

		[Test]
		public void ShouldReturnMessageCount()
		{
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

			var target = new MessageController(null, null, pushMessageProvider);

			var messageInfo = new MessagesInformationViewModel { UnreadMessagesCount = 1 };
			pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(messageInfo.UnreadMessagesCount);

			var result = target.MessagesCount();

			((MessagesInformationViewModel)result.Data).UnreadMessagesCount.Should().Be.EqualTo(messageInfo.UnreadMessagesCount);
		}

		[Test]
		public void ShouldReturnMessageCountAndMessageAskedFor()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			var messageViewModel = new MessageViewModel { MessageId = Guid.NewGuid().ToString() };
			var messageInfo = new MessagesInformationViewModel
								{
									UnreadMessagesCount = 1,
									MessageItem = messageViewModel
								};

			viewModelFactory.Stub(x => x.CreateMessagesInformationViewModel(new Guid(messageViewModel.MessageId))).Return(messageInfo);

			var target = new MessageController(viewModelFactory, null, null);
			var result = target.Message(new MessageForm {MessageId = messageViewModel.MessageId});

			result.Data.Should().Be.SameInstanceAs(messageInfo);
		}
	}
}
