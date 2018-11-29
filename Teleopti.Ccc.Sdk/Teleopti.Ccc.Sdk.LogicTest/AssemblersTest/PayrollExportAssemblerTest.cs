using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
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
    public class PayrollExportAssemblerTest
    {
		private readonly PayrollFormatDto _payrollFormat = new PayrollFormatDto(Guid.NewGuid(), "format");

		[Test]
	    public void VerifyDoToDto()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
		    var personRepository = new FakePersonRepositoryLegacy();
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    var payrollExportRepository = new FakePayrollExportRepository();
		    var target = new PayrollExportAssembler(payrollExportRepository, personRepository, personAssembler,
			    new DateTimePeriodAssembler());

		    var payrollExport = createDo(person);

		    payrollExport.Name = "Tommie";
		    var result = target.DomainEntityToDto(payrollExport);
		    Assert.AreEqual(payrollExport.Id, result.Id);
		    Assert.AreEqual(payrollExport.Period.StartDate.Date, result.DatePeriod.StartDate.DateTime);
		    Assert.AreEqual(payrollExport.Period.StartDate.Date, result.Period.LocalStartDateTime.Date);
		    Assert.AreEqual(payrollExport.Persons[0].Id, result.PersonCollection[0].Id);
		    Assert.AreEqual(payrollExport.Name, result.Name);
	    }

	    [Test]
	    public void VerifyDoToDtoWithNoPersonAssembler()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
		    
		    var personRepository = new FakePersonRepositoryLegacy();
		    var payrollExportRepository = new FakePayrollExportRepository();
		    var payrollExport = createDo(person);

		    var target = new PayrollExportAssembler(payrollExportRepository, personRepository, null,
			    new DateTimePeriodAssembler());
		    var result = target.DomainEntityToDto(payrollExport);
		    Assert.AreEqual(payrollExport.Id, result.Id);
		    Assert.AreEqual(payrollExport.Period.StartDate.Date, result.DatePeriod.StartDate.DateTime);
		    Assert.AreEqual(payrollExport.Period.StartDate.Date, result.Period.LocalStartDateTime.Date);
		    Assert.AreEqual(payrollExport.Persons[0].Id, result.ExportPersonCollection[0]);
	    }

	    [Test]
	    public void VerifyDtoToDo()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
		    
		    var personRepository = new FakePersonRepositoryLegacy();
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    var payrollExportRepository = new FakePayrollExportRepository();
		    var payrollExport = createDo(person);
		    payrollExportRepository.Add(payrollExport);
		    var target = new PayrollExportAssembler(payrollExportRepository, personRepository, personAssembler,
			    new DateTimePeriodAssembler());

		    var payrollExportDto = createDto(payrollExport.Id.GetValueOrDefault(),person);
		    var result = target.DtoToDomainEntity(payrollExportDto);
		    Assert.AreEqual(payrollExportDto.Id, result.Id);
		    Assert.AreEqual(payrollExportDto.DatePeriod.StartDate.DateTime, result.Period.StartDate.Date);
	    }

	    [Test]
	    public void VerifyDtoToDoWithNoId()
	    {
		    var personRepository = new FakePersonRepositoryLegacy();
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    var payrollExportRepository = new FakePayrollExportRepository();
		    var target = new PayrollExportAssembler(payrollExportRepository, personRepository, personAssembler,
			    new DateTimePeriodAssembler());

		    var result = target.DtoToDomainEntity(new PayrollExportDto());
		    Assert.IsNull(result);
	    }

	    [Test]
	    public void VerifyDtoToDoWithIdListOnly()
	    {
		    var person = PersonFactory.CreatePerson().WithId();

		    var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);

		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    var payrollExportRepository = new FakePayrollExportRepository();
		    var payrollExport = createDo(person);
		    var target = new PayrollExportAssembler(payrollExportRepository, personRepository, personAssembler,
			    new DateTimePeriodAssembler());

		    var payrollExportDto = createDto(payrollExport.Id.GetValueOrDefault(), person);
		    payrollExportDto.PersonCollection.Clear();
		    payrollExportDto.ExportPersonCollection.Add(person.Id.GetValueOrDefault());
		    var result = target.DtoToDomainEntity(payrollExportDto);
		    Assert.AreEqual(1, result.Persons.Count);
	    }

	    private IPayrollExport createDo(IPerson person)
        {
            IPayrollExport payrollExport = new PayrollExport().WithId();
            payrollExport.AddPersons(new List<IPerson> { person });
            payrollExport.Period = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue);
            payrollExport.PayrollFormatId = _payrollFormat.FormatId;
            payrollExport.PayrollFormatName = _payrollFormat.Name;
            payrollExport.FileFormat = ExportFormat.CommaSeparated;
            ReflectionHelper.SetCreatedBy(payrollExport,person);
            return payrollExport;
        }

        private PayrollExportDto createDto(Guid exportId, IPerson person)
        {
            PayrollExportDto payrollExportDto = new PayrollExportDto();
            payrollExportDto.Id = exportId;
            payrollExportDto.PayrollFormat = _payrollFormat;
	        payrollExportDto.DatePeriod = new DateOnlyPeriodDto
		        {StartDate = new DateOnlyDto {DateTime = DateOnly.MinValue.Date}, EndDate = new DateOnlyDto {DateTime = DateOnly.MinValue.Date } };
	        payrollExportDto.PersonCollection.Add(new PersonDto {Id = person.Id, Name = person.Name.ToString()});
            return payrollExportDto;
        }
    }
}
