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
			var target = new MessageController(MockRepository.GenerateMock<IMessageViewModelFactory>(), null, TODO);

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("MessagePartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForMessages()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			var target = new MessageController(viewModelFactory, null, TODO);
			var model = new MessageViewModel[] { };
			var paging = new Paging();

			viewModelFactory.Stub(x => x.CreatePageViewModel(paging)).Return(model);

			var result = target.Messages(paging);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldReplyToSimpleMessage()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
			var pushMessageDialoguePersister = MockRepository.GenerateMock<IPushMessageDialoguePersister>();
			
			var target = new MessageController(viewModelFactory, pushMessageDialoguePersister, TODO);

			var messageViewModel = new MessageViewModel {MessageId = Guid.NewGuid().ToString()};
			var form = new MessageForm {MessageId = messageViewModel.MessageId};

			pushMessageDialoguePersister.Stub(x => x.Persist(messageViewModel.MessageId)).Return(messageViewModel);

			var result = target.Reply(form);

			result.Data.Should().Be.SameInstanceAs(messageViewModel);
		}

        [Test]
        public void ShouldReturnMessageCount()
        {
            var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
            var pushMessageDialoguePersister = MockRepository.GenerateMock<IPushMessageDialoguePersister>();
            var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();

            var target = new MessageController(viewModelFactory, pushMessageDialoguePersister, pushMessageProvider);

            var messageInfo = new MessageInfo {UnreadMessagesCount = 1};
            pushMessageProvider.Stub(x => x.UnreadMessageCount).Return(messageInfo.UnreadMessagesCount);

            var result = target.MessagesCount();

            result.Data.Should().Be.SameInstanceAs(messageInfo);
        }
	}
}
