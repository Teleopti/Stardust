using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class DialogueMessageAssemblerTest
    {
	    [Test]
	    public void VerifyDoToDto()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
		    var personRepository = new FakePersonRepository();
		    personRepository.Has(person);

			var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));
		    var target = new DialogueMessageAssembler();
		    target.PersonAssembler = personAssembler;
		    var dialogueMessage = createDo(person);

		    var result = target.DomainEntityToDto(dialogueMessage);
		    Assert.AreEqual(dialogueMessage.Sender.Id, result.Sender.Id);
		    Assert.AreEqual(dialogueMessage.Text, result.Text);
	    }

	    [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDo()
        {
            var dialogueMessageDto = createDto();
			var target = new DialogueMessageAssembler();

			target.DtoToDomainEntity(dialogueMessageDto);
        }

        private static IDialogueMessage createDo(IPerson person)
        {
            var dialogueMessage = new DialogueMessage("test", person);
            return dialogueMessage;
        }

        private static DialogueMessageDto createDto()
        {
            return new DialogueMessageDto();
        }
    }
}
