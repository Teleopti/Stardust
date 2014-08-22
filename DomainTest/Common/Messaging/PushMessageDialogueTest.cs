using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common.Messaging
{
    [TestFixture, SetUICulture("en-US")]
    public class PushMessageDialogueTest
    {
        private IPushMessageDialogue _target;
        private IPerson _person;
        private IPushMessage _pushMessage;
        private MockRepository _mocker;
        private CultureInfo _swedish;
        private CultureInfo _english;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _person = new Person();
            _pushMessage = new PushMessage();
            _target = new PushMessageDialogue(_pushMessage, _person);
            _swedish = CultureInfo.GetCultureInfoByIetfLanguageTag("sv-SE");
            _english = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_pushMessage, _target.PushMessage);
            Assert.AreEqual(_person, _target.Receiver);
        }

        [Test]
        public void VerifyCanAddMessage()
        {
            const string messageText = "Hello";
            _target.DialogueReply(messageText, _person);
            Assert.AreEqual(messageText, _target.DialogueMessages[0].Text);
            Assert.AreEqual(_person, _target.DialogueMessages[0].Sender);
            Assert.GreaterOrEqual(DateTime.UtcNow, _target.DialogueMessages[0].Created);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifySenderMustNotBeNull()
        {
            const string messageText = "Hello mr nullperson";
            _target.DialogueReply(messageText, null);
        }

        [Test]
        public void VerifyMainReference()
        {
            Assert.AreSame(_person, _target.MainRoot);
        }

        [Test]
        [Ignore]
        public void VerifyReply()
        {
            string replyNotOk = "replyNotOk";
            string replyOK = "replyOK";
            string replyOK2 = "replyOK2";

            IPushMessage replyChecker = _mocker.StrictMock<IPushMessage>();
            _target = new PushMessageDialogue(replyChecker, _person);
            Assert.IsFalse(_target.IsReplied);
            using (_mocker.Record())
            {
                Expect.Call(replyChecker.CheckReply(replyNotOk)).Return(false);
                Expect.Call(replyChecker.CheckReply(replyOK)).Return(true);
                Expect.Call(replyChecker.CheckReply(replyOK2)).Return(true);
            }
            using (_mocker.Playback())
            {
                _target.SetReply(replyNotOk);
                Assert.IsFalse(_target.IsReplied);
				Assert.AreNotEqual(_target.GetReply(new NoFormatting()), replyNotOk);

                _target.SetReply(replyOK);
                Assert.IsTrue(_target.IsReplied);
				Assert.AreEqual(replyOK, _target.GetReply(new NoFormatting()));

                _target.SetReply(replyOK2);
				Assert.AreEqual(replyOK, _target.GetReply(new NoFormatting()), "To make sure its not called if Replied is True");
            }
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyCannotReplyWithTextOnMessageNotAllowingReplyText()
        {
            _pushMessage.AllowDialogueReply = false;
            _target.DialogueReply("This is a text message", _person);
        }

        [Test]
        public void VerifyClear()
        {
			string defaultReply = _target.GetReply(new NoFormatting());
            _pushMessage.ReplyOptions.Add("ok");
            _target.SetReply("ok");
            Assert.IsTrue(_target.IsReplied);

            _target.ClearReply();
			Assert.AreEqual(defaultReply, _target.GetReply(new NoFormatting()));
            Assert.IsFalse(_target.IsReplied);
        }

        [Test]
        public void VerifyThatTheDialogueIsNotRepliedIfSomeoneElseThanTheReceiverAddsAMessage()
        {
			string defaultReply = _target.GetReply(new NoFormatting());
            IPerson anotherPerson = PersonFactory.CreatePerson();
            _pushMessage.ReplyOptions.Add("ok");
            _target.SetReply("ok");

            Assert.IsTrue(_target.IsReplied);
            _target.DialogueReply("blah", _person);

            Assert.IsTrue(_target.IsReplied, "If the receiver adds a message, the reply should still be set");

            _target.DialogueReply("blah", anotherPerson);
            Assert.IsFalse(_target.IsReplied, "target is cleared if a message is added from ANY other person");
			Assert.AreEqual(defaultReply, _target.GetReply(new NoFormatting()));
        }

        [Test]
        public void VerifyTranslateMessageReturnsOriginalTextIfNotInResources()
        {
            _pushMessage.TranslateMessage = true;
            _pushMessage.Message = "a text that is guaranteed to not exist in resources eftersom halva texten är på english ock makes no sence";

			var noFormatting = new NoFormatting();
			Assert.AreEqual(_target.Message(noFormatting), _pushMessage.GetMessage(noFormatting));
        }

        [Test]
        public void VerifyTranslates()
        {
            IPerson swedishPerson = PersonFactory.CreatePerson("person1");
            IPerson englishPerson = PersonFactory.CreatePerson("person2");
            
            swedishPerson.PermissionInformation.SetUICulture(_swedish);
            englishPerson.PermissionInformation.SetUICulture(_english);
            
            string str = Resources.Delete;
            var swedishString = Resources.ResourceManager.GetString(str, swedishPerson.PermissionInformation.UICulture());
            var englishString = Resources.ResourceManager.GetString(str, englishPerson.PermissionInformation.UICulture());

            Assert.AreNotEqual(swedishString,englishString,"Make sure they are different, otherwise this test will not really check anything");

	        var notTranslatedPushMessage = new PushMessage {Message = str, TranslateMessage = false};
            var translatedPushMessage = new PushMessage { Message = str, TranslateMessage = true };
          
            PushMessageDialogue dialogue1 = new PushMessageDialogue(notTranslatedPushMessage,swedishPerson);
            PushMessageDialogue dialogue2 = new PushMessageDialogue(notTranslatedPushMessage,englishPerson);
            PushMessageDialogue dialogue3 = new PushMessageDialogue(translatedPushMessage, swedishPerson);
            PushMessageDialogue dialogue4 = new PushMessageDialogue(translatedPushMessage,englishPerson);

	        var noFormatting = new NoFormatting();
	        Assert.AreEqual(dialogue1.Message(noFormatting), englishString, "Should be english because not translated");
			Assert.AreEqual(dialogue2.Message(noFormatting), englishString, "Should be english because not translated");
			Assert.AreEqual(dialogue3.Message(noFormatting), swedishString, "Should be swedish because translated and receiver has swedish UICulture");
			Assert.AreEqual(dialogue4.Message(noFormatting), englishString, "Should be english because  translated, but receiver has english UICulture");
        }
    }
}
