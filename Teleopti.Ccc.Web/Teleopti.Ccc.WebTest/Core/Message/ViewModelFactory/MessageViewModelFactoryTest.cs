using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.ViewModelFactory
{
	[TestFixture]
	public class MessageViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnZeroViewModelsFromPushMessages()
		{
			var messageProvider = MockRepository.GenerateMock<IPushMessageProvider>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var paging = new Paging();

			var target = new MessageViewModelFactory(messageProvider, mapper);

			var domainMessages = new List<IPushMessageDialogue>();
			messageProvider.Stub(x => x.GetMessages(paging)).Return(domainMessages);
			mapper.Stub(x => x.Map<IList<IPushMessageDialogue>, IList<MessageViewModel>>(domainMessages)).Return(
				new List<MessageViewModel>());

			IList<MessageViewModel> result = target.CreatePageViewModel(paging);

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetUnreadMessagesCountAndAGivenMessage()
		{
			var messageProvider = MockRepository.GenerateMock<IPushMessageProvider>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			IPerson person = new Person();

			var domainMessage = new PushMessageDialogue(new PushMessage(new[] { "OK" }), person);
			domainMessage.SetId(Guid.NewGuid());
			var messageViewModel = new MessageViewModel { MessageId = domainMessage.Id.ToString() };

			messageProvider.Stub(x => x.GetMessage(domainMessage.Id.Value)).Return(domainMessage);
			messageProvider.Stub(x => x.UnreadMessageCount).Return(1);


			mapper.Stub(x => x.Map<IPushMessageDialogue, MessageViewModel>(domainMessage)).Return(messageViewModel);

			var target = new MessageViewModelFactory(messageProvider, mapper);
			MessagesInformationViewModel result = target.CreateMessagesInformationViewModel(domainMessage.Id.Value);
			
			result.MessageItem.Should().Be.SameInstanceAs(messageViewModel);
			result.UnreadMessagesCount.Should().Be.EqualTo(1);
		}
	}
}
