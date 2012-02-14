﻿using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PayrollExportAssembler : Assembler<IPayrollExport,PayrollExportDto>
    {
        private readonly IPayrollExportRepository _payrollExportRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public PayrollExportAssembler(IPayrollExportRepository payrollExportRepository, IPersonRepository personRepository, IAssembler<IPerson,PersonDto> personAssembler, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _payrollExportRepository = payrollExportRepository;
            _personRepository = personRepository;
            _personAssembler = personAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override PayrollExportDto DomainEntityToDto(IPayrollExport entity)
        {
            var timeZone = entity.CreatedBy.PermissionInformation.DefaultTimeZone();
            PayrollExportDto dto = new PayrollExportDto();
            dto.Id = entity.Id;
            dto.PayrollFormat = new PayrollFormatDto(entity.PayrollFormatId, entity.PayrollFormatName);
            dto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period.ToDateTimePeriod(timeZone));
            dto.DatePeriod = new DateOnlyPeriodDto(entity.Period);
            dto.TimeZoneId = timeZone.Id;
            dto.Name = entity.Name;
            foreach (IPerson person in entity.Persons)
            {
                dto.ExportPersonCollection.Add(person.Id.GetValueOrDefault());
            }
            if (_personAssembler != null)
            {
                var personDtos = _personAssembler.DomainEntitiesToDtos(entity.Persons);
                foreach (PersonDto person in personDtos)
                {
                    dto.PersonCollection.Add(person);
                }
            }

            return dto;
        }

        public override IPayrollExport DtoToDomainEntity(PayrollExportDto dto)
        {
            IPayrollExport payrollExport = null;
            if (HasNoExportPersonCollectionSet(dto))
            {
                payrollExport = _payrollExportRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
            }
            if (HasExportPersonCollectionSet(dto))
            {
                var allPersons = _personRepository.FindAllSortByName();
                var allPersonsForPayrollExport =
                    allPersons.Where(p => dto.ExportPersonCollection.Contains(p.Id.GetValueOrDefault()));

                payrollExport = new PayrollExport();
                payrollExport.SetId(dto.Id);
                SetCreatedBy(payrollExport, TeleoptiPrincipal.Current.GetPerson(_personRepository));
                payrollExport.AddPersons(allPersonsForPayrollExport);
            }
            if (payrollExport == null) return null;

            payrollExport.Period = new DateOnlyPeriod(new DateOnly(dto.DatePeriod.StartDate.DateTime), new DateOnly(dto.DatePeriod.EndDate.DateTime));
            payrollExport.PayrollFormatId = dto.PayrollFormat.FormatId;
            payrollExport.PayrollFormatName = dto.PayrollFormat.Name;

            return payrollExport;
        }

        private static bool HasNoExportPersonCollectionSet(PayrollExportDto dto)
        {
            return dto.ExportPersonCollection == null || dto.ExportPersonCollection.Count == 0;
        }

        private static bool HasExportPersonCollectionSet(PayrollExportDto dto)
        {
            return dto.ExportPersonCollection!=null && dto.ExportPersonCollection.Count>0;
        }

        private static void SetCreatedBy(ICreateInfo aggregateRoot, IPerson person)
        {
            typeof(AggregateRoot).GetField("_createdBy", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                aggregateRoot, person);
        }
    }
}
