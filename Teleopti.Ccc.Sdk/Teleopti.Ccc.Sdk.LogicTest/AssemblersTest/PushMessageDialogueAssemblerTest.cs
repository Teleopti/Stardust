using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PushMessageDialogueAssemblerTest
    {
	    [Test]
	    public void VerifyDoToDto()
	    {
		    var receiver = PersonFactory.CreatePerson("Da receiver").WithId();
		    var person = PersonFactory.CreatePerson().WithId();
		    var personRepository = new FakePersonRepositoryLegacy();
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var activityRepository = new FakeActivityRepository();
		    var activityAssembler = new ActivityAssembler(activityRepository);
		    var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
		    var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));
		    var pushMessageAssembler = new PushMessageAssembler {PersonAssembler = personAssembler};
		    var dialogueMessageAssembler = new DialogueMessageAssembler {PersonAssembler = personAssembler};
		    var target = new PushMessageDialogueAssembler();
		    target.PersonAssembler = personAssembler;
		    target.DialogueMessageAssembler = dialogueMessageAssembler;
		    target.PushMessageAssembler = pushMessageAssembler;

			IList<string> replyOptions = new List<string> { "OK", "NOK" };
			IPushMessage pushMessage = new PushMessage(replyOptions)
			{
				Title = "Vem kan jobba?",
				Message = "Vi är underbemannade och behöver extrainsatser.",
				Sender = person
			};
			IPushMessageDialogue pushMessageDialogue = new PushMessageDialogue(pushMessage, receiver);
			pushMessageDialogue.DialogueReply("Glöm det!!", person);
			
		    var result = target.DomainEntityToDto(pushMessageDialogue);
		    Assert.AreEqual(1, result.Messages.Count);
		    Assert.AreEqual(person.Id, result.PushMessage.Sender.Id);
		    Assert.AreEqual("Da receiver Da receiver", result.Receiver.Name);
		    IList<DialogueMessageDto> dialogueMessageDtos = (IList<DialogueMessageDto>) result.Messages;
		    Assert.AreEqual("Glöm det!!", dialogueMessageDtos[0].Text);
	    }

	    [Test]
	    public void ShouldTransformToDtoWithInvalidCharacter()
	    {
			var receiver = PersonFactory.CreatePerson("Da receiver").WithId();
			var person = PersonFactory.CreatePerson().WithId();
			var personRepository = new FakePersonRepositoryLegacy();
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var activityRepository = new FakeActivityRepository();
			var activityAssembler = new ActivityAssembler(activityRepository);
			var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
			var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var pushMessageAssembler = new PushMessageAssembler { PersonAssembler = personAssembler };
			var dialogueMessageAssembler = new DialogueMessageAssembler {PersonAssembler = personAssembler};
		    var target = new PushMessageDialogueAssembler
		    {
			    PersonAssembler = personAssembler,
			    DialogueMessageAssembler = dialogueMessageAssembler,
			    PushMessageAssembler = pushMessageAssembler
		    };

		    IList<string> replyOptions = new List<string> { "OK", "NOK" };
			IPushMessage pushMessage = new PushMessage(replyOptions)
			{
				Title = "Vem kan jobba?",
				Message = "Vi är underbemannade och behöver extrainsatser.",
				Sender = person
			};
			IPushMessageDialogue pushMessageDialogue = new PushMessageDialogue(pushMessage, receiver);
			pushMessageDialogue.DialogueReply("Glöm det!!", person);
			pushMessageDialogue.PushMessage.Message = "invalid character: ";

		    var result = target.DomainEntityToDto(pushMessageDialogue);
		    result.Message.Should().Not.Contain("");
	    }

	    [Test]
        public void VerifyDtoToDo()
        {
			var personRepository = new FakePersonRepositoryLegacy();
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var activityRepository = new FakeActivityRepository();
			var activityAssembler = new ActivityAssembler(activityRepository);
			var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
			var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var pushMessageAssembler = new PushMessageAssembler { PersonAssembler = personAssembler };
			var dialogueMessageAssembler = new DialogueMessageAssembler {PersonAssembler = personAssembler};
		    var target = new PushMessageDialogueAssembler
		    {
			    PersonAssembler = personAssembler,
			    DialogueMessageAssembler = dialogueMessageAssembler,
			    PushMessageAssembler = pushMessageAssembler
		    };

		    var dialogueMessageDto = new PushMessageDialogueDto();
            Assert.Throws<NotSupportedException>(() => target.DtoToDomainEntity(dialogueMessageDto));
        }
    }
}
