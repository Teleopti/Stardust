using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonDayOffAssemblerTest
    {
        private PersonDayOffAssembler _target;

        private IPerson _person;
        private IScenario _scenario;
        private TimeZoneInfo _timeZoneInfo;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _target = new PersonDayOffAssembler(_personAssembler,new DateTimePeriodAssembler());
            _target.Person = _person;
            _target.DefaultScenario = _scenario;
        }

        [Test]
        public void VerifyDoToDto()
        {
            var date = new DateOnly(1900, 1, 1);
            var anchorTime = new TimeSpan(23);
            var flexibility = new TimeSpan(124);
            var length = new TimeSpan(1244);

						var dayOff = DayOffFactory.CreateDayOff(new Description("test"));
					dayOff.Anchor = anchorTime;
						dayOff.SetTargetAndFlexibility(length, flexibility);
						var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_scenario, _person, date, dayOff);


            personDayOff.SetId(Guid.NewGuid());

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
                var dayOffDto = _target.DomainEntityToDto(personDayOff);

                Assert.AreEqual(personDayOff.Person.Id.Value, dayOffDto.Person.Id.Value);
                Assert.AreEqual(personDayOff.Id.Value, dayOffDto.Id.Value);
                Assert.AreEqual(personDayOff.Period.StartDateTime, dayOffDto.Period.UtcStartTime);
            }
        }
    }
}