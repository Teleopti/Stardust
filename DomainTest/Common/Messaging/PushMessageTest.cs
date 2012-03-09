using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common.Messaging
{
    [TestFixture]
    public class PushMessageTest
    {
        private IPushMessage _target;

        [SetUp]
        public void Setup()
        {
            _target = new PushMessage();
        }
	
        [Test]
        public void VerifyConstructorAndProperties()
        {
            string title = "Title";
            string message = "Message";
            Assert.IsTrue(_target.AllowDialogueReply);
            _target.Title = title;
            _target.Message = message;
            _target.AllowDialogueReply = false;
			Assert.AreEqual(title, _target.GetTitle(new NoFormatting()));
			Assert.AreEqual(message, _target.GetMessage(new NoFormatting()));
            Assert.IsFalse(_target.AllowDialogueReply);
        }

        [Test]
        public void VerifyReplyOptions()
        {
            IList<string> replyOptions = new List<string> {"a", "b"};
            _target = new PushMessage(replyOptions);
            Assert.Contains("a",(ICollection) _target.ReplyOptions);
            Assert.Contains("b",(ICollection) _target.ReplyOptions);
        }

        [Test]
        public void VerifyCheckReply()
        {
            IList<string> replyOptions = new List<string> { "a", "b" };
            _target = new PushMessage(replyOptions);
            Assert.IsFalse(_target.CheckReply("c"));
            Assert.IsTrue(_target.CheckReply("a"));
        }

        [Test]
        public void ShouldBeAbleToHandleXmlSafeCharactersOtherwiseTheWebServiceCrashesWhenParsingTheMessageToXml()
        {
            //They have managed to insert some strange invisible chars (Group Separator) between the words КАНАЛ and Premium
            _target.Message = "Premium Digital.;КОЛЕГИ ИМА HS ЗА ПРЕУСТАНОВЯВАНЕ ИЗЛЪЧВАНЕТО НА КАНАЛ  Premium Digital.ПРОЧЕТЕТЕ!";
            _target.Title = "Premium Digital.;КОЛЕГИ ИМА HS ЗА ПРЕУСТАНОВЯВАНЕ ИЗЛЪЧВАНЕТО НА КАНАЛ  Premium Digital.ПРОЧЕТЕТЕ!";
            
            //To check that we can parse xml
            var xml = "<?xml version=\"1.0\"?>" +
                "<PushMessage>" +
					"<Message>" + _target.GetMessage(new NormalizeText()) + "</Message>" +
					"<Title>" + _target.GetTitle(new NormalizeText()) + "</Title>" +
                "</PushMessage>";
            using (var s = new StringReader(xml))
            {
                var t = new XmlTextReader(s);
                while (t.Read())
                {
                }
            }

            //The unvisible chars has been converted to xml-friendly 
			_target.GetMessage(new NormalizeText()).Should().Be.EqualTo("Premium Digital.;КОЛЕГИ ИМА HS ЗА ПРЕУСТАНОВЯВАНЕ ИЗЛЪЧВАНЕТО НА КАНАЛ &#x1D; Premium Digital.ПРОЧЕТЕТЕ!");
			_target.GetTitle(new NormalizeText()).Should().Be.EqualTo("Premium Digital.;КОЛЕГИ ИМА HS ЗА ПРЕУСТАНОВЯВАНЕ ИЗЛЪЧВАНЕТО НА КАНАЛ &#x1D; Premium Digital.ПРОЧЕТЕТЕ!");
        }
    }
}
