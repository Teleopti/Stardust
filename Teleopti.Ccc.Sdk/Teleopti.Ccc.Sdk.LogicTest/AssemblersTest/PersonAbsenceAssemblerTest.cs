using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAbsenceAssemblerTest 
    {
	    [Test]
	    public void VerifyDtoToDo()
	    {
		    var absenceRepository = new FakeAbsenceRepository();
		    var absenceAssembler = new AbsenceAssembler(absenceRepository);
		    var dateTimePeriodAssembler = new DateTimePeriodAssembler();
		    var target = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
		    var person = PersonFactory.CreatePerson();
		    var scenario = ScenarioFactory.CreateScenarioAggregate();
		    target.Person = person;
		    target.DefaultScenario = scenario;

		    var dto = new PersonAbsenceDto {Id = Guid.NewGuid(), Version = 4};
		    var absenceDto = new AbsenceDto {Id = Guid.NewGuid()};

		    dto.AbsenceLayer = new AbsenceLayerDto();
		    dto.AbsenceLayer.Id = Guid.NewGuid();
		    dto.AbsenceLayer.Period = dateTimePeriodAssembler.DomainEntityToDto(new DateTimePeriod(1900, 1, 1, 1900, 1, 2));
		    dto.AbsenceLayer.Absence = absenceDto;

		    var absence = AbsenceFactory.CreateAbsence("abs").WithId(absenceDto.Id.Value);
		    absenceRepository.Add(absence);

		    IPersonAbsence entity = target.DtoToDomainEntity(dto);

		    Assert.AreEqual(dto.Version, entity.Version);
		    Assert.AreEqual(dto.Id, entity.Id);
		    Assert.AreSame(absence, entity.Layer.Payload);
		    Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 1900, 1, 2), entity.Layer.Period);
	    }

	    [Test]
        public void CannotExecuteFromDtoConverterWithoutPerson()
        {
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var target = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			target.DefaultScenario = scenario;
			target.Person = null;

            Assert.Throws<InvalidOperationException>(() => target.DtoToDomainEntity(new PersonAbsenceDto()));
        }

        [Test]
        public void CannotExecuteFromDtoConverterWithoutDefaultScenario()
		{
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var target = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var person = PersonFactory.CreatePerson();

			target.Person = person;
			target.DefaultScenario = null;

            Assert.Throws<InvalidOperationException>(() => target.DtoToDomainEntity(new PersonAbsenceDto()));
        }

        [Test]
        public void VerifyDoToDto()
		{
	        var absenceRepository = new FakeAbsenceRepository();
	        var absenceAssembler = new AbsenceAssembler(absenceRepository);
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var target = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			target.Person = person;
			target.DefaultScenario = scenario;

			IPersonAbsence personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
                                                                                    new DateTimePeriod(1900, 1, 1, 1900,
                                                                                                       1, 2)).WithId();
			
            absenceRepository.Add(personAbsence.Layer.Payload.WithId());
			
            PersonAbsenceDto personAbsenceDto = target.DomainEntityToDto(personAbsence);
            Assert.AreEqual(person.Id.Value, personAbsence.Person.Id.Value);
            Assert.AreEqual(personAbsence.Id.Value, personAbsenceDto.Id.Value);
            Assert.AreEqual(personAbsence.Layer.Payload.Id.Value, personAbsenceDto.AbsenceLayer.Absence.Id.Value);
        }
    }
}