using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonMeetingAssemblerTest
    {
        private PersonMeetingAssembler _target;
        private IPerson _person;
        private IScenario _scenario;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _person = PersonFactory.CreatePerson();
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _target = new PersonMeetingAssembler(_personAssembler,new DateTimePeriodAssembler());
            _target.DefaultScenario = _scenario;
            _target.Person = _person;
        }

        [Test]
        public void VerifyDoToDto()
        {
            Assert.IsNotNull(_target);

            DateTime dateTime = new DateTime(1999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
            IPersonMeeting personMeeting = CreatePersonMeeting(dateTimePeriod, _person);

            using(_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
                                                                                    {
                                                                                        Id = _person.Id,
                                                                                        Name = _person.Name.ToString()
                                                                                    });
            }
            using (_mocks.Playback())
            {
                PersonMeetingDto personMeetingDto = _target.DomainEntityToDto(personMeeting);

                Assert.AreEqual(0, personMeetingDto.Period.UtcStartTime.CompareTo(dateTime));
                Assert.AreEqual(personMeetingDto.Person.Id, _target.Person.Id);
            }
        }

        private static IPersonMeeting CreatePersonMeeting(DateTimePeriod period, IPerson person)
        {
            IMeeting mainMeeting = new Meeting(new Person(), new List<IMeetingPerson>(), "subject", "location", "description",
                    new Activity("activity"), new Scenario("scenario"));
            IPersonMeeting personMeeting = new PersonMeeting(mainMeeting, new MeetingPerson(person, true), period);
            personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(person, true));
            personMeeting.SetId(Guid.NewGuid());
            return personMeeting;
        }

        [Test]
        public void VerifyDtoToDo()
        {
            DateTime dateTime = new DateTime(1999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
            IPersonMeeting personMeeting = CreatePersonMeeting(dateTimePeriod, _person);
            using (_mocks.Record())
            {
                var personDto = new PersonDto
                                    {
                                        Id = _person.Id,
                                        Name = _person.Name.ToString()
                                    };
                Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(personDto);
            }
            using (_mocks.Playback())
            {
                PersonMeetingDto personMeetingDto = _target.DomainEntityToDto(personMeeting);
                IPersonMeeting newPersonMeeting = _target.DtoToDomainEntity(personMeetingDto);
                Assert.IsNotNull(newPersonMeeting);
                Assert.AreEqual(newPersonMeeting.Id, personMeetingDto.Id);
                Assert.AreEqual(newPersonMeeting.Id, personMeeting.Id);
            }
        }
    }
}
