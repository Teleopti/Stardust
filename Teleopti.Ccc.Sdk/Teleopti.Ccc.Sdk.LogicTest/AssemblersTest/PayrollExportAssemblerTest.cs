using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PayrollExportAssemblerTest
    {
        private IPerson _person;
        private PayrollFormatDto _payrollFormat;
        private readonly Guid _payrollExportGuid = Guid.NewGuid();
        private DateOnly _dateMin;
        private MockRepository _mocks;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private PayrollExportAssembler _target;
        private IPayrollExportRepository _payrollExportRepository;
        private IPersonRepository _personRepository;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _payrollFormat = new PayrollFormatDto(Guid.NewGuid(), "format");
            _dateMin = new DateOnly(DateTime.MinValue);
            _mocks = new MockRepository();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _payrollExportRepository = _mocks.StrictMock<IPayrollExportRepository>();
            _personRepository = _mocks.StrictMock<IPersonRepository>();
            _target = new PayrollExportAssembler(_payrollExportRepository, _personRepository, _personAssembler, new DateTimePeriodAssembler());
        }

        [Test]
        public void VerifyDoToDto()
        {
            var payrollExport = CreateDo();
            using (_mocks.Record())
            {
                Expect.Call(_personAssembler.DomainEntitiesToDtos(new List<IPerson> {_person})).Return(new List<PersonDto>
                                                                                                           {
                                                                                                               new PersonDto
                                                                                                                   {
                                                                                                                       Id = _person.Id,
                                                                                                                       Name = _person.Name.ToString()
                                                                                                                   }
                                                                                                           });
            }
            using (_mocks.Playback())
            {
                payrollExport.Name = "Tommie";
                var result = _target.DomainEntityToDto(payrollExport);
                Assert.AreEqual(payrollExport.Id,result.Id);
                Assert.AreEqual(payrollExport.Period.StartDate.Date,result.DatePeriod.StartDate.DateTime);
                Assert.AreEqual(payrollExport.Period.StartDate.Date, result.Period.LocalStartDateTime.Date);
                Assert.AreEqual(payrollExport.Persons[0].Id,result.PersonCollection[0].Id);
                Assert.AreEqual(payrollExport.Name,result.Name);
            }
        }

        [Test]
        public void VerifyDoToDtoWithNoPersonAssembler()
        {
            var payrollExport = CreateDo();
            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                _target = new PayrollExportAssembler(_payrollExportRepository, _personRepository, null, new DateTimePeriodAssembler());
                var result = _target.DomainEntityToDto(payrollExport);
                Assert.AreEqual(payrollExport.Id, result.Id);
                Assert.AreEqual(payrollExport.Period.StartDate.Date, result.DatePeriod.StartDate.DateTime);
                Assert.AreEqual(payrollExport.Period.StartDate.Date, result.Period.LocalStartDateTime.Date);
                Assert.AreEqual(payrollExport.Persons[0].Id, result.ExportPersonCollection[0]);
            }
        }

        [Test]
        public void VerifyDtoToDo()
        {
            using (_mocks.Record())
            {
                Expect.Call(_payrollExportRepository.Get(_payrollExportGuid)).Return(CreateDo());
            }
            using (_mocks.Playback())
            {
                var payrollExportDto = CreateDto();
                var result = _target.DtoToDomainEntity(payrollExportDto);
                Assert.AreEqual(payrollExportDto.Id, result.Id);
                Assert.AreEqual(payrollExportDto.DatePeriod.StartDate.DateTime, result.Period.StartDate.Date);
            }
        }

        [Test]
        public void VerifyDtoToDoWithNoId()
        {
            using (_mocks.Record())
            {
                Expect.Call(_payrollExportRepository.Get(Guid.Empty)).Return(null);
            }
            using (_mocks.Playback())
            {
                var payrollExportDto = CreateDto();
                payrollExportDto.Id = null;
                var result = _target.DtoToDomainEntity(payrollExportDto);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void VerifyDtoToDoWithIdListOnly()
        {
            var principalPerson = ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person;
            using (_mocks.Record())
            {
                Expect.Call(_personRepository.FindAllSortByName()).Return(new List<IPerson>{_person, PersonFactory.CreatePerson()});
                Expect.Call(_personRepository.Get(principalPerson.Id.GetValueOrDefault())).Return(principalPerson);
            }
            using (_mocks.Playback())
            {
                var payrollExportDto = CreateDto();
                payrollExportDto.PersonCollection.Clear();
                payrollExportDto.ExportPersonCollection.Add(_person.Id.GetValueOrDefault());
                var result = _target.DtoToDomainEntity(payrollExportDto);
                Assert.AreEqual(1, result.Persons.Count);
            }
        }

        private IPayrollExport CreateDo()
        {
            IPayrollExport payrollExport = new PayrollExport();
            payrollExport.AddPersons(new List<IPerson> { _person });
            payrollExport.Period = new DateOnlyPeriod(_dateMin, _dateMin);
            payrollExport.PayrollFormatId = _payrollFormat.FormatId;
            payrollExport.PayrollFormatName = _payrollFormat.Name;
            payrollExport.FileFormat = ExportFormat.CommaSeparated;
            payrollExport.SetId(_payrollExportGuid);
            ReflectionHelper.SetCreatedBy(payrollExport,_person);
            return payrollExport;
        }

        private PayrollExportDto CreateDto()
        {
            PayrollExportDto payrollExportDto = new PayrollExportDto();
            payrollExportDto.Id = _payrollExportGuid;
            payrollExportDto.PayrollFormat = _payrollFormat;
	        payrollExportDto.DatePeriod = new DateOnlyPeriodDto
		        {StartDate = new DateOnlyDto {DateTime = _dateMin}, EndDate = new DateOnlyDto {DateTime = _dateMin}};
            payrollExportDto.PersonCollection.Add(new PersonDto{Id=_person.Id,Name = _person.Name.ToString()});
            return payrollExportDto;
        }
    }
}
