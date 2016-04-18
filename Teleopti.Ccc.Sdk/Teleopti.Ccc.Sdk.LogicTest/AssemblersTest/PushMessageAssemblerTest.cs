using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
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
    public class PushMessageAssemblerTest
    {
	    [Test]
	    public void VerifyDoToDto()
	    {
			var personRepository = new FakePersonRepository();
		    var person = PersonFactory.CreatePerson().WithId();
		    personRepository.Add(person);

			var pushMessage = new PushMessage(new List<string> { "Yes", "No" }).WithId();
			pushMessage.Sender = person;
			pushMessage.Title = "title";
			pushMessage.Message = "message";

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
		    var target = new PushMessageAssembler();
		    target.PersonAssembler = personAssembler;

		    var result = target.DomainEntityToDto(pushMessage);
		    Assert.AreEqual(pushMessage.Sender.Id, result.Sender.Id);
		    Assert.AreEqual(pushMessage.GetMessage(new NormalizeText()), result.Message);
		    Assert.AreEqual(pushMessage.GetTitle(new NormalizeText()), result.Title);
		    Assert.AreEqual(2, result.ReplyOptions.Count);
	    }

	    [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDo()
        {
            var dialogueMessageDto = new PushMessageDto();
			var target = new PushMessageAssembler();
			target.DtoToDomainEntity(dialogueMessageDto);
        }
    }
}
