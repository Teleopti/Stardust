using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAbsenceAssemblerTest 
    {
        private PersonAbsenceAssembler target;
        private MockRepository mocks;
        private IPerson person;
        private IScenario scenario;
        private IAssembler<IAbsence, AbsenceDto> absenceAssembler;
        private DateTimePeriodAssembler dateTimePeriodAssembler;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            absenceAssembler = mocks.StrictMock<IAssembler<IAbsence, AbsenceDto>>();
            dateTimePeriodAssembler = new DateTimePeriodAssembler();
            target = new PersonAbsenceAssembler(absenceAssembler,dateTimePeriodAssembler);
            person = PersonFactory.CreatePerson();
            scenario = ScenarioFactory.CreateScenarioAggregate();
            target.Person = person;
            target.DefaultScenario = scenario;
        }

        [Test]
        public void VerifyDtoToDo()
        {
            PersonAbsenceDto dto = new PersonAbsenceDto { Id = Guid.NewGuid(), Version = 4 };
            AbsenceDto absenceDto = new AbsenceDto{Id=Guid.NewGuid()};
            
            dto.AbsenceLayer = new AbsenceLayerDto();
            dto.AbsenceLayer.Id = Guid.NewGuid();
            dto.AbsenceLayer.Period = dateTimePeriodAssembler.DomainEntityToDto(new DateTimePeriod(1900, 1, 1, 1900, 1, 2));
            dto.AbsenceLayer.Absence = absenceDto;

            IAbsence absence = AbsenceFactory.CreateAbsence("abs");

            using (mocks.Record())
            {
                Expect.Call(absenceAssembler.DtoToDomainEntity(dto.AbsenceLayer.Absence)).Return(absence);
            }
            using (mocks.Playback())
            {
                IPersonAbsence entity = target.DtoToDomainEntity(dto);

                Assert.AreEqual(dto.Version, entity.Version);
                Assert.AreEqual(dto.Id, entity.Id);
                Assert.AreSame(absence, entity.Layer.Payload);
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 1900, 1, 2), entity.Layer.Period);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutPerson()
        {
            target.Person = null;
            target.DtoToDomainEntity(new PersonAbsenceDto());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutDefaultScenario()
        {
            target.DefaultScenario = null;
            target.DtoToDomainEntity(new PersonAbsenceDto());
        }

        [Test]
        public void VerifyDoToDto()
        {
            IPersonAbsence personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
                                                                                    new DateTimePeriod(1900, 1, 1, 1900,
                                                                                                       1, 2));

            person.SetId(Guid.NewGuid());
            personAbsence.SetId(Guid.NewGuid());
            personAbsence.Layer.Payload.SetId(Guid.NewGuid());

            Expect.Call(absenceAssembler.DomainEntityToDto(personAbsence.Layer.Payload)).Return(new AbsenceDto
                                                                                            {
                                                                                                Id = personAbsence.Layer.Payload.Id
                                                                                            });

            mocks.ReplayAll();
            PersonAbsenceDto personAbsenceDto = target.DomainEntityToDto(personAbsence);
            Assert.AreEqual(person.Id.Value, personAbsence.Person.Id.Value);
            Assert.AreEqual(personAbsence.Id.Value, personAbsenceDto.Id.Value);
            Assert.AreEqual(personAbsence.Layer.Payload.Id.Value, personAbsenceDto.AbsenceLayer.Absence.Id.Value);
            mocks.VerifyAll();
        }
    }
}