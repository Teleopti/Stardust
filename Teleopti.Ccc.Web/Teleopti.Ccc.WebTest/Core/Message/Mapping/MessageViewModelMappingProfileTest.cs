using System;
using System.Collections.Generic;
using System.Globalization;
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
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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
    	private IPerson _person;
    	private PushMessageDialogue _pushMessageDialogue;

    	[SetUp]
        public void Setup()
        {
            _person = new Person {Name = new Name("ashley", "andeen")};
			_person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
            _pushMessage = new PushMessage()
                                  {
                                      Title = "my title",
                                      Message = "message text",
                                      //AllowDialogueReply = true,
                                      //TranslateMessage = true,
                                      Sender = _person,
                                      
                                  };
            _pushMessageDialogue = new PushMessageDialogue(_pushMessage, _person);
			SetDate(_pushMessageDialogue, DateTime.Now, "_updatedOn");

            _domainMessages = new[] {_pushMessageDialogue};

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
        	var dateTimeString = _pushMessageDialogue.UpdatedOn.Value.ToShortDateTimeString();
			_result.First().Date.Should().Be.EqualTo(dateTimeString);
        }

        public static void SetDate(IAggregateRoot root, DateTime? dateTime, string property)
        {
            IChangeInfo rootCheck = root as IChangeInfo;
            if (rootCheck != null)
            {
                Type rootType = typeof(AggregateRoot);
                if (dateTime.HasValue)
                    rootType.GetField(property, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(root, dateTime);
            }
        }
    }
}
