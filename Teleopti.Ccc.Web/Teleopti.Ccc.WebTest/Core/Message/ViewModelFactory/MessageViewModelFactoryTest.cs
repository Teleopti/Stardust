using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core.Message.ViewModelFactory
{
	[TestFixture]
	public class MessageViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnZeroViewModelsFromPushMessages()
		{
			var messageDialogueRepository = new FakePushMessageDialogueRepository();
			var messageProvider = new PushMessageProvider(new FakeLoggedOnUser(),messageDialogueRepository);
			var paging = new Paging();

			var target = new MessageViewModelFactory(messageProvider, new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeUserTimeZone());
			
			IList<MessageViewModel> result = target.CreatePageViewModel(paging);

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetUnreadMessagesCountAndAGivenMessage()
		{
			var messageDialogueRepository = new FakePushMessageDialogueRepository();
			var messageProvider = new PushMessageProvider(new FakeLoggedOnUser(), messageDialogueRepository);
			IPerson person = new Person();

			var domainMessage = new PushMessageDialogue(new PushMessage(new[] { "OK" }), person).WithId();
			messageDialogueRepository.Add(domainMessage);
			
			var target = new MessageViewModelFactory(messageProvider, new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeUserTimeZone());
			MessagesInformationViewModel result = target.CreateMessagesInformationViewModel(domainMessage.Id.Value);

			result.UnreadMessagesCount.Should().Be.EqualTo(1);
		}
	}
}
