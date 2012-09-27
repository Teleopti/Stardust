using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.Mapping
{
    [TestFixture]
    public class MessageViewModelMappingProfileTest
    {
        private IList<IPushMessageDialogue> _domainMessages;
        private PushMessage _pushMessage;
        private IList<MessageViewModel> _result;

        [SetUp]
        public void Setup()
        {
            var person = new Person {Name = new Name("ashley", "andeen")};
            _pushMessage = new PushMessage()
                                  {
                                      Title = "my title",
                                      Message = "message text",
                                      //AllowDialogueReply = true,
                                      //TranslateMessage = true,
                                      Sender = person,
                                      
                                  };
            SetCreatedOn(_pushMessage, DateTime.Now);
            var pushMessageDialogue = new PushMessageDialogue(_pushMessage, new Person());

            _domainMessages = new[] {pushMessageDialogue};

            Mapper.Reset();
            Mapper.Initialize(c => c.AddProfile(new MessageViewModelMappingProfile()));

            _result = Mapper.Map<IList<IPushMessageDialogue>, IList<MessageViewModel>>(_domainMessages);
        }


        [Test]
        public void ShouldConfigure()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void ShouldMapOneMesageCount()
        {
            _result.Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldMapTitle()
        {
            _result.First().Title.Should().Be.EqualTo(_pushMessage.GetTitle(new NoFormatting()));
        }

        [Test]
        public void ShouldMapMessage()
        {
            _result.First().Message.Should().Be.EqualTo(_pushMessage.GetMessage(new NoFormatting()));
        }

        //[Test]
        //public void ShouldMapAllowDialogueReply()
        //{
        //    _result.First().AllowDialogueReply.Should().Be.EqualTo(_pushMessage.AllowDialogueReply);
        //}
        
        //[Test]
        //public void ShouldMapTranslateMessage()
        //{
        //    _result.First().TranslateMessage.Should().Be.EqualTo(_pushMessage.TranslateMessage);
        //}

        [Test]
        public void ShouldMapSender()
        {
            _result.First().Sender.Should().Be.EqualTo(_pushMessage.Sender.Name.ToString());
        }

        [Test]
        public void ShouldMapDate()
        {
            _result.First().Date.Should().Be.EqualTo(_pushMessage.CreatedOn);
        }

        public static void SetCreatedOn(IAggregateRoot root, DateTime? createdOn)
        {
            IChangeInfo rootCheck = root as IChangeInfo;
            if (rootCheck != null)
            {
                Type rootType = typeof(AggregateRoot);
                if (createdOn.HasValue)
                    rootType.GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, createdOn);
            }
        }
    }
}
