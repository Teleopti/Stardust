using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PushMessageAssemblerTest
    {
        private IPerson _person;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private PushMessageAssembler _target;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _target = new PushMessageAssembler();
            _target.PersonAssembler = _personAssembler;
        }

        [Test]
        public void VerifyDoToDto()
        {
            var dialogueMessage = CreateDo();
            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
                                                                                    {
                                                                                        Id = _person.Id,
                                                                                        Name = _person.Name.ToString()
                                                                                    });
            }
            using (_mocks.Playback())
            {
                var result = _target.DomainEntityToDto(dialogueMessage);
                Assert.AreEqual(dialogueMessage.Sender.Id, result.Sender.Id);
                Assert.AreEqual(dialogueMessage.GetMessage(new NormalizeText()), result.Message);
                Assert.AreEqual(dialogueMessage.GetTitle(new NormalizeText()), result.Title);
                Assert.AreEqual(2,result.ReplyOptions.Count);
            }
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDo()
        {
            var dialogueMessageDto = CreateDto();
            _target.DtoToDomainEntity(dialogueMessageDto);
        }

        private IPushMessage CreateDo()
        {
            IPushMessage pushMessage = new PushMessage(new List<string>{"Yes","No"});
            pushMessage.SetId(Guid.NewGuid());
            pushMessage.Sender = _person;
            pushMessage.Title = "title";
            pushMessage.Message = "message";
            return pushMessage;
        }

        private static PushMessageDto CreateDto()
        {
            return new PushMessageDto();
        }
    }
}
