﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Time;
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
    	private ICccTimeZoneInfo _cccTimeZone;

    	[SetUp]
        public void Setup()
    	{
			var timeZone = MockRepository.GenerateMock<IUserTimeZone>();
			
			_person = new Person {Name = new Name("ashley", "andeen")};
            _pushMessage = new PushMessage()
                                  {
                                      Title = "my title",
                                      Message = "message text",
                                      //AllowDialogueReply = true,
                                      //TranslateMessage = true,
                                      Sender = _person,
                                      
                                  };
            _pushMessageDialogue = new PushMessageDialogue(_pushMessage, _person);
			SetDate(_pushMessageDialogue, DateTime.UtcNow, "_updatedOn");

            _domainMessages = new[] {_pushMessageDialogue};

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new MessageViewModelMappingProfile(
				() => timeZone
				)));

			_cccTimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			timeZone.Stub(x => x.TimeZone()).Return(_cccTimeZone);

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
        public void ShouldMapMessageToShowOnlyThirtyCharacters()
        {
        	_pushMessage.Message = "this is the long message text that should be truncated";
			_result = Mapper.Map<IList<IPushMessageDialogue>, IList<MessageViewModel>>(_domainMessages);

        	const int length = 50;
        	var shortMessage = _pushMessage.GetMessage(new NoFormatting()).Substring(0, length) + "...";
            _result.First().Message.Should().Be.EqualTo(shortMessage);
        }

		[Test]
		public void ShouldMapMessageToShowShortMessage()
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
        	var localDateTimeString = _cccTimeZone.ConvertTimeFromUtc(_pushMessageDialogue.UpdatedOn.Value).ToShortDateTimeString();
			_result.First().Date.Should().Be.EqualTo(localDateTimeString);
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
