using System;
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
    public class DialogueMessageAssemblerTest
    {
        private IPerson _person;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private DialogueMessageAssembler _target;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _target = new DialogueMessageAssembler();
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
                Assert.AreEqual(dialogueMessage.Text, result.Text);
            }
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDo()
        {
            var dialogueMessageDto = CreateDto();
            _target.DtoToDomainEntity(dialogueMessageDto);
        }

        private IDialogueMessage CreateDo()
        {
            IDialogueMessage dialogueMessage = new DialogueMessage("test",_person);
            return dialogueMessage;
        }

        private static DialogueMessageDto CreateDto()
        {
            return new DialogueMessageDto();
        }
    }
}
