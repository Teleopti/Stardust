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
        public void VerifyDtoToDo()
        {
            var dto = new PersonDayOffDto
                                      {
                                          Id = Guid.NewGuid(),
                                          Version = 4,
                                          Anchor = new DateTime(2009, 1, 1, 11, 0, 0, DateTimeKind.Utc),
                                          AnchorTime = new TimeSpan(12, 0, 0),
                                          Flexibility = new TimeSpan(0),
                                          Length = new TimeSpan(24, 0, 0),
                                          Name = "Malmös Maradona",
                                          Person = null,
                                          //totalt onödig eftersom den tillhör en schedulepart
                                          ShortName = "banana",
                                          Color = new ColorDto(Color.Blue),
										  PayrollCode = "payrollcode007"
                                      };


            var entity = _target.DtoToDomainEntity(dto);
            Assert.AreEqual(dto.Id, entity.Id);
            Assert.AreEqual(dto.Version, entity.Version);
            Assert.AreEqual(dto.Anchor, entity.DayOff.Anchor);
            Assert.AreEqual(dto.AnchorTime, entity.DayOff.AnchorLocal(StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone).TimeOfDay);
            Assert.AreEqual(dto.Flexibility, entity.DayOff.Flexibility);
            Assert.AreEqual(dto.Length, entity.DayOff.TargetLength);
            Assert.AreEqual(dto.Name, entity.DayOff.Description.Name);
            Assert.AreEqual(_person.Id, entity.Person.Id);
            Assert.AreEqual(dto.ShortName, entity.DayOff.Description.ShortName);
            Assert.AreEqual(dto.Color.ToColor(), entity.DayOff.DisplayColor);
            Assert.AreEqual(dto.PayrollCode, entity.DayOff.PayrollCode);
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
            var personDayOff = new PersonDayOff(_person, _scenario, dayOff, date);


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