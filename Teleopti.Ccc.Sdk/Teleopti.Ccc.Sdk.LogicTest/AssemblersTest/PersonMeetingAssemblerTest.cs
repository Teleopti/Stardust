using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonMeetingAssemblerTest
    {
	    [Test]
	    public void VerifyDoToDto()
	    {
		    var scenario = ScenarioFactory.CreateScenarioAggregate();
		    var person = PersonFactory.CreatePerson();

		    var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    
		    var dateTime = new DateTime(1999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		    var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
		    var personMeeting = createPersonMeeting(dateTimePeriod, person);

			var target = new PersonMeetingAssembler(personAssembler, new DateTimePeriodAssembler());
			target.DefaultScenario = scenario;
			target.Person = person;
			var personMeetingDto = target.DomainEntityToDto(personMeeting);

		    Assert.AreEqual(0, personMeetingDto.Period.UtcStartTime.CompareTo(dateTime));
		    Assert.AreEqual(personMeetingDto.Person.Id, target.Person.Id);
	    }

	    private static IPersonMeeting createPersonMeeting(DateTimePeriod period, IPerson person)
        {
            IMeeting mainMeeting = new Meeting(new Person(), new List<IMeetingPerson>(), "subject", "location", "description",
                    new Activity("activity"), new Scenario("scenario")).WithId();
            IPersonMeeting personMeeting = new PersonMeeting(mainMeeting, new MeetingPerson(person, true), period);
            personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(person, true));
            return personMeeting;
        }

	    [Test]
	    public void VerifyDtoToDo()
	    {
		    var scenario = ScenarioFactory.CreateScenarioAggregate();
		    var person = PersonFactory.CreatePerson();

		    var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

		    var dateTime = new DateTime(1999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		    var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
		    var personMeeting = createPersonMeeting(dateTimePeriod, person);

			var target = new PersonMeetingAssembler(personAssembler, new DateTimePeriodAssembler());
			target.DefaultScenario = scenario;
			target.Person = person;

			var personMeetingDto = target.DomainEntityToDto(personMeeting);
		    var newPersonMeeting = target.DtoToDomainEntity(personMeetingDto);
		    Assert.IsNotNull(newPersonMeeting);
	    }
    }
}
