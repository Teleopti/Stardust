using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.WebTest.Core.Common;
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
			var paging = new Paging();

			var target = new MessageViewModelFactory(messageProvider, new FakePersonNameProvider(), new FakeUserTimeZone());

			var domainMessages = new List<IPushMessageDialogue>();
			messageProvider.Stub(x => x.GetMessages(paging)).Return(domainMessages);
			
			IList<MessageViewModel> result = target.CreatePageViewModel(paging);

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetUnreadMessagesCountAndAGivenMessage()
		{
			var messageProvider = MockRepository.GenerateMock<IPushMessageProvider>();
			IPerson person = new Person();

			var domainMessage = new PushMessageDialogue(new PushMessage(new[] { "OK" }), person);
			domainMessage.SetId(Guid.NewGuid());
			
			messageProvider.Stub(x => x.GetMessage(domainMessage.Id.Value)).Return(domainMessage);
			messageProvider.Stub(x => x.UnreadMessageCount).Return(1);
			
			var target = new MessageViewModelFactory(messageProvider, new FakePersonNameProvider(), new FakeUserTimeZone());
			MessagesInformationViewModel result = target.CreateMessagesInformationViewModel(domainMessage.Id.Value);

			result.UnreadMessagesCount.Should().Be.EqualTo(1);
		}
	}
}
