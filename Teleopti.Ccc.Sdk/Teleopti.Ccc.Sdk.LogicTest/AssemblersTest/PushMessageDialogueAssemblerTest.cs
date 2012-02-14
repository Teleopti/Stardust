using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PushMessageDialogueAssemblerTest
    {
        private IPerson _person;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private PushMessageDialogueAssembler _target;
        private IPerson _receiver;
        private IAssembler<IPushMessage, PushMessageDto> _pushMessageAssembler;
        private IAssembler<IDialogueMessage, DialogueMessageDto> _dialogueMessageAssembler;

        [SetUp]
        public void Setup()
        {
            _receiver = PersonFactory.CreatePerson("Da receiver");
            _receiver.SetId(Guid.NewGuid());
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _pushMessageAssembler = _mocks.StrictMock<IAssembler<IPushMessage, PushMessageDto>>();
            _dialogueMessageAssembler = _mocks.StrictMock<IAssembler<IDialogueMessage, DialogueMessageDto>>();
            _target = new PushMessageDialogueAssembler();
            _target.PersonAssembler = _personAssembler;
            _target.DialogueMessageAssembler = _dialogueMessageAssembler;
            _target.PushMessageAssembler = _pushMessageAssembler;
        }

        [Test]
        public void VerifyDoToDto()
        {
            var dialogueMessage = CreateDo();
            using (_mocks.Record())
            {
                var senderDto = new PersonDto
                                    {
                                        Id = _person.Id,
                                        Name = _person.Name.ToString()
                                    };
                var recieverDto = new PersonDto
                                      {
                                          Id = _receiver.Id,
                                          Name = _receiver.Name.ToString(),
                                          TimeZoneId = TimeZoneInfo.Utc.Id
                };
                Expect.Call(_personAssembler.DomainEntityToDto(_receiver)).Return(recieverDto);
                Expect.Call(_dialogueMessageAssembler.DomainEntityToDto(dialogueMessage.DialogueMessages[0])).Return(
                    new DialogueMessageDto{Text = dialogueMessage.DialogueMessages[0].Text});
                Expect.Call(_pushMessageAssembler.DomainEntityToDto(dialogueMessage.PushMessage)).Return(
                    new PushMessageDto {Sender = senderDto});
            }
            using (_mocks.Playback())
            {
                var result = _target.DomainEntityToDto(dialogueMessage);
                Assert.AreEqual(1, result.Messages.Count);
                Assert.AreEqual(_person.Id, result.PushMessage.Sender.Id);
                Assert.AreEqual("Da receiver Da receiver", result.Receiver.Name);
                IList<DialogueMessageDto> dialogueMessageDtos = (IList<DialogueMessageDto>)result.Messages;
                Assert.AreEqual("Glöm det!!", dialogueMessageDtos[0].Text);
            }
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDo()
        {
            var dialogueMessageDto = CreateDto();
            _target.DtoToDomainEntity(dialogueMessageDto);
        }

        private IPushMessageDialogue CreateDo()
        {
            IList<string> replyOptions = new List<string> { "OK", "NOK" };
            IPushMessage pushMessage = new PushMessage(replyOptions)
            {
                Title = "Vem kan jobba?",
                Message = "Vi är underbemannade och behöver extrainsatser.",
                Sender = _person
            };
            IPushMessageDialogue pushMessageDialogue = new PushMessageDialogue(pushMessage, _receiver);
            pushMessageDialogue.DialogueReply("Glöm det!!", _person);
            return pushMessageDialogue;
        }

        private static PushMessageDialogueDto CreateDto()
        {
            return new PushMessageDialogueDto();
        }
    }
}
