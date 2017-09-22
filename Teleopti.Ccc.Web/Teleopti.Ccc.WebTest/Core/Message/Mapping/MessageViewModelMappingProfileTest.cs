using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core.Message.Mapping
{
	[TestFixture]
	public class MessageViewModelMappingProfileTest
	{
		private PushMessage _pushMessage;
		private IPerson _person;
		private PushMessageDialogue _pushMessageDialogue;
		private TimeZoneInfo _cccTimeZone;
		private IPerson _replier;
		private IPersonNameProvider _personNameProvider;
		private MessageViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			var timeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_cccTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			timeZone.Stub(x => x.TimeZone()).Return(_cccTimeZone);

			_replier = new Person();
			_replier.SetId(Guid.Empty);
			_replier.WithName(new Name("Ashley","Andeen"));
			_person = new Person().WithName(new Name("ashley", "andeen"));
			_pushMessage = new PushMessage(new[] { "OK" })
										 {
											 Title = "my title",
											 Message = "message text",
											 AllowDialogueReply = true,
											 Sender = _person
										 };
			_pushMessage.ReplyOptions.Add("Yes");
			_pushMessage.ReplyOptions.Add("No");
			_pushMessage.ReplyOptions.Add("Maybe");
			_pushMessageDialogue = new PushMessageDialogue(_pushMessage, _person);
			_pushMessageDialogue.SetId(Guid.NewGuid());
			_pushMessageDialogue.DialogueMessages.Add(new DialogueMessage("A reply", _replier));
			_pushMessageDialogue.DialogueMessages.Add(new DialogueMessage("Another reply", _replier));
			SetDate(_pushMessageDialogue, DateTime.UtcNow, "_updatedOn");
			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_person.Name)).Return(_person.Name.FirstName + " " + _person.Name.LastName);
			
			target = new MessageViewModelMapper(timeZone, _personNameProvider);
		}
		
		[Test]
		public void ShouldMapOneMesageCount()
		{
			var result = target.Map(_pushMessageDialogue);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapSenderOfDialogueMessage()
		{
			var result = target.Map(_pushMessageDialogue);

			result.DialogueMessages.First().SenderId.Should().Be.EqualTo(_replier.Id);
		}

		[Test]
		public void ShouldMapTitle()
		{
			var result = target.Map(_pushMessageDialogue);

			result.Title.Should().Be.EqualTo(_pushMessage.GetTitle(new NoFormatting()));
		}

		[Test]
		public void ShouldMapMessageType()
		{
			var result = target.Map(_pushMessageDialogue);

			result.MessageType.Should().Be.EqualTo((int)_pushMessage.MessageType);
		}

		[Test]
		public void ShouldMapMessageToShowShortMessage()
		{
			var result = target.Map(_pushMessageDialogue);

			result.Message.Should().Be.EqualTo(_pushMessage.GetMessage(new NoFormatting()));
		}

		[Test]
		public void ShouldMapAllowDialogueReply()
		{
			var result = target.Map(_pushMessageDialogue);

			result.AllowDialogueReply.Should().Be.EqualTo(_pushMessage.AllowDialogueReply);
		}

		[Test]
		public void ShouldMapDialogueMessages()
		{
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_replier.Name)).Return(_replier.Name.FirstName + " " + _replier.Name.LastName);

			var result = target.Map(_pushMessageDialogue);

			result.DialogueMessages.First().Text.Should().Be.EqualTo(_pushMessageDialogue.DialogueMessages.First().Text);
			result.DialogueMessages.First().Sender.Should().Be.EqualTo(_pushMessageDialogue.DialogueMessages.First().Sender.Name.FirstName + " " + _pushMessageDialogue.DialogueMessages.First().Sender.Name.LastName);
			var localDateTimeString = TimeZoneInfo.ConvertTimeFromUtc(_pushMessageDialogue.DialogueMessages.First().Created,_cccTimeZone).ToShortDateTimeString();
			result.DialogueMessages.First().Created.Should().Be.EqualTo(localDateTimeString);
		}

		[Test]
		public void ShouldMapReplyOptions()
		{
			var result = target.Map(_pushMessageDialogue);

			result.ReplyOptions.Count.Should().Be.EqualTo(_pushMessageDialogue.PushMessage.ReplyOptions.Count);
		}

		[Test]
		public void ShouldMapSender()
		{
			var result = target.Map(_pushMessageDialogue);

			result.Sender.Should().Be.EqualTo(_pushMessage.Sender.Name.FirstName + " " + _pushMessage.Sender.Name.LastName);
		}

		[Test]
		public void ShouldMapDate()
		{
			var result = target.Map(_pushMessageDialogue);

			var localDateTimeString = TimeZoneInfo.ConvertTimeFromUtc(_pushMessageDialogue.UpdatedOn.Value,_cccTimeZone).ToShortDateTimeString();
			result.Date.Value.ToShortDateTimeString().Should().Be.EqualTo(localDateTimeString);
		}

		[Test]
		public void ShouldMapMessageId()
		{
			var result = target.Map(_pushMessageDialogue);

			result.MessageId.Should().Be.EqualTo(_pushMessageDialogue.Id.ToString());
		}

		[Test]
		public void ShouldMapMessageIsRead()
		{
			var result = target.Map(_pushMessageDialogue);

			_pushMessageDialogue.SetReply(_pushMessage.ReplyOptions.First());
			result = target.Map(_pushMessageDialogue);
			result.IsRead.Should().Be.True();
		}

        [Test]
        public void ShouldTranslateMessageForReceiver()
        {
            _pushMessage.TranslateMessage = true;
            _pushMessage.Message = "TextRequestHasBeenDeniedDot";

            var result = target.Map(_pushMessageDialogue);
            result.Message.Should().Be.EqualTo(UserTexts.Resources.TextRequestHasBeenDeniedDot);
        }

		public static void SetDate(IAggregateRoot root, DateTime dateTime, string property)
		{
			var rootCheck = root as IChangeInfo;
			if (rootCheck != null)
			{
				Type rootType = typeof(AggregateRoot);
				rootType.GetField(property, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, dateTime);
			}
		}
	}
}
